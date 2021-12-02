import logging
import os
import time
import typing as _t
from enum import Enum
from threading import RLock

import flask
from werkzeug.exceptions import HTTPException

from utils import get_console_logger


SERVICE_NAME = 'rep-log-secondary'

# Delay in sec for POST requests, default is no delay
POST_DELAY: _t.Optional[int] = None

# This flag can be used to test messages consistency
RETURN_ERROR_BEFORE_EVEN_MESSAGE = False

# This flag can be used to test message deduplication
RETURN_ERROR_AFTER_EVEN_MESSAGE = False

LOGGER: _t.Optional[logging.Logger] = None

app = flask.Flask(SERVICE_NAME)

# Ready to display queue of messages
messages_queue: _t.List[str] = []
# Buffer contains messages that cannot yet be added into message queue because of delays
messages_buffer: _t.Dict[int, str] = {}
# Using lock on messages queue and buffer makes sure they are safe across multiple threads
# NOTE: Can be acquired multiple times by the same thread
messages_lock = RLock()


class ResponseStatus(Enum):
    error = 'error'
    ok = 'ok'


def get_response(
    status: ResponseStatus,
    status_code: int = 200,
    msg: _t.Optional[str] = None,
    verbose: bool = True,
    **kwargs,
) -> flask.Response:
    """Return json response with all additional information inside"""
    response = dict(status=status.value, **kwargs)

    if msg is not None:
        response['message'] = msg

    # TODO: Print endpoint and method
    if verbose:
        endpoint = f'{flask.request.method} {flask.request.path}'
        LOGGER.debug(f'Response: {response} Endpoint: {endpoint} Status code: {status_code}')

    return flask.jsonify(response), status_code


def get_error_response(msg: str, status_code: int = 500, **kwargs) -> flask.Response:
    """Log stacktrace with error before the response"""
    LOGGER.exception(msg)
    return get_response(status=ResponseStatus.error, status_code=status_code, msg=msg, **kwargs)


@app.errorhandler(Exception)
def handle_error(err: Exception) -> flask.Response:
    """Handle unexpected error"""
    status_code = 500
    if isinstance(err, HTTPException):
        status_code = err.code
    return get_error_response(msg=str(err), status_code=status_code)


@app.route("/health", methods=['GET'])
def check_health() -> flask.Response:
    """Check service health"""
    return get_response(status=ResponseStatus.ok, verbose=False)


@app.route("/", methods=['GET'])
def get_messages() -> flask.Response:
    """Get all messages from the queue"""
    LOGGER.info('Get all messages ...')
    return get_response(status=ResponseStatus.ok, messages=messages_queue)


def is_next_message(message_id: int) -> bool:
    """Check if this is the next message, based on the assumption that messages start with id = 1"""
    return message_id == len(messages_queue) + 1


def try_add_from_buffer() -> None:
    """Try add messages from buffer into the queue"""
    if not messages_buffer:
        # Buffer is empty
        return

    # Iterate from the earliest to the latest message in buffer
    for message_id, message in sorted(messages_buffer.items()):
        if not is_next_message(message_id):
            # Next message is not yet in the buffer, all further messages can be skipped
            break

        LOGGER.info(f'[id={message_id}] Move message from buffer into queue ...')
        messages_queue.append(message)
        del messages_buffer[message_id]


def handle_message(message_id: int, message: str) -> None:
    """Add message into the queue or buffer"""
    with messages_lock:

        # Deduplication
        is_duplicated = message_id <= len(messages_queue)
        if is_duplicated:
            LOGGER.warning(f'[id={message_id}] Message with such id already exists!')
            return

        if is_next_message(message_id):
            LOGGER.info(f'[id={message_id}] Add new message into queue ...')
            messages_queue.append(message)
            try_add_from_buffer()
        else:
            # Add into buffer since there are delayed messages
            LOGGER.info(f'[id={message_id}] Add new message into buffer ...')
            messages_buffer[message_id] = message

        LOGGER.info(f'[id={message_id}] There are {len(messages_queue)} messages in queue.')
        LOGGER.info(f'[id={message_id}] There are {len(messages_buffer)} messages in buffer.')


@app.route("/", methods=['POST'])
def append_message() -> flask.Response:
    """Append new message to the local queue"""

    try:
        request_body = flask.request.get_json()
    except TypeError:
        return get_error_response(msg='Could not parse input json!')

    try:
        messages = request_body['messages']
    except KeyError:
        return get_error_response('Not found "messages" in input json!')
    except TypeError:
        return get_error_response('Could not get data from request! Possible reasons: missing Content-Type in headers')

    try:
        message_ids = [int(message_id) for message_id in request_body['ids']]
    except KeyError:
        return get_error_response('Not found field "id" inside input json!')
    except ValueError:
        return get_error_response('Could not parse field "ids" from input json!')

    if len(message_ids) != len(messages):
        return get_error_response('Number of messages is not the same as number of ids!')

    if POST_DELAY > 0:
        LOGGER.info(f'Sleep for {POST_DELAY} seconds ...')
        time.sleep(POST_DELAY)

    if RETURN_ERROR_BEFORE_EVEN_MESSAGE and sum(message_ids) % 2 == 0:
        # Error is returned if sum(message_id) is even and messages are not added into the queue
        return get_error_response(msg='Testing error before even message!')

    LOGGER.info(f'Will try to add {len(messages)} messages ...')
    for message_id, message in sorted(zip(message_ids, messages)):
        handle_message(message_id, message)

    # Error is returned if sum(message_id) is even after messages were added into the queue
    status = ResponseStatus.error if RETURN_ERROR_AFTER_EVEN_MESSAGE else ResponseStatus.ok
    status_code = 500 if RETURN_ERROR_AFTER_EVEN_MESSAGE and sum(message_ids) % 2 == 0 else 200

    return get_response(status=status, status_code=status_code)


def run_app() -> None:
    global SERVICE_NAME, POST_DELAY, RETURN_ERROR_BEFORE_EVEN_MESSAGE, RETURN_ERROR_AFTER_EVEN_MESSAGE, LOGGER

    host = os.environ['SECONDARY_HOST']
    port = int(os.environ['SECONDARY_PORT'])

    # Override service name if needed, f.e for logging
    SERVICE_NAME = os.getenv('SERVICE_NAME', 'rep-log-secondary')
    POST_DELAY = int(os.getenv('POST_DELAY', 0))
    RETURN_ERROR_BEFORE_EVEN_MESSAGE = bool(os.getenv('ERROR_BEFORE_EVEN_MESSAGE', 'false') == 'true')
    RETURN_ERROR_AFTER_EVEN_MESSAGE = bool(os.getenv('ERROR_AFTER_EVEN_MESSAGE', 'false') == 'true')

    # Disable built-in Flask logging
    logging.getLogger('werkzeug').setLevel(logging.ERROR)
    LOGGER = get_console_logger(SERVICE_NAME, level=logging.DEBUG)

    app.run(host=host, port=port, debug=False)


def main() -> None:
    run_app()


if __name__ == '__main__':
    main()
