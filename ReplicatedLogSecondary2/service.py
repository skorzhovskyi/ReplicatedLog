
import logging
import os
import time
import typing as _t
from enum import Enum
from threading import RLock

import flask

from utils import get_console_logger


class ResponseStatus(Enum):
    error = 'error'
    ok = 'ok'


service_name = 'replicated-log-secondary-2'

app = flask.Flask(service_name)
logger: _t.Optional[logging.Logger] = None

# Ready to display queue of messages
messages_queue: _t.List[str] = []
# Buffer contains messages that cannot yet be added into message queue because of delays
messages_buffer: _t.Dict[int, str] = {}
# Using lock on messages queue and buffer makes sure they are safe across multiple threads
# NOTE: Can be acquired multiple times by the same thread
messages_lock = RLock()

# Delay in sec for POST requests, default is no delay
post_delay: _t.Optional[int] = None

# This flag can be used to test messages consistency
return_error_before_even_message = False

# This flag can be used to test message deduplication
return_error_after_even_message = False


def get_response(
    status: ResponseStatus,
    status_code: int = 200,
    msg: _t.Optional[str] = None,
    **kwargs,
) -> flask.Response:
    """Return json response with all additional information inside"""
    response = dict(status=status.value, **kwargs)

    if msg is not None:
        response['message'] = msg

    # TODO: Print endpoint and method
    logger.debug(f'Response: {response} Status code: {status_code}')

    return flask.jsonify(response), status_code


def get_error_response(msg: str, status_code: int = 500, **kwargs) -> flask.Response:
    """Log stacktrace with error before the response"""
    logger.exception(msg)
    return get_response(status=ResponseStatus.error, status_code=status_code, msg=msg, **kwargs)


@app.route("/health", methods=['GET'])
def check_health() -> flask.Response:
    """Check service health"""
    return get_response(status=ResponseStatus.ok)


@app.route("/", methods=['GET'])
def get_messages() -> flask.Response:
    """Get all messages from the queue"""
    logger.info('Get all messages ...')
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

        logger.info(f'[id={message_id}] Move message from buffer into queue ...')
        messages_queue.append(message)
        del messages_buffer[message_id]


def handle_message(message_id: int, message: str) -> None:
    """Add message into the queue or buffer"""
    with messages_lock:

        # Deduplication
        is_duplicated = message_id <= len(messages_queue)
        if is_duplicated:
            logger.warning(f'[id={message_id}] Message with such id already exists!')
            return

        if is_next_message(message_id):
            logger.info(f'[id={message_id}] Add new message into queue ...')
            messages_queue.append(message)
            try_add_from_buffer()
        else:
            # Add into buffer since there are delayed messages
            logger.info(f'[id={message_id}] Add new message into buffer ...')
            messages_buffer[message_id] = message

        logger.info(f'[id={message_id}] There are {len(messages_queue)} messages in queue.')
        logger.info(f'[id={message_id}] There are {len(messages_buffer)} messages in buffer.')


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

    if post_delay > 0:
        logger.info(f'Sleep for {post_delay} seconds ...')
        time.sleep(post_delay)

    if return_error_before_even_message and sum(message_ids) % 2 == 0:
        # Error is returned if sum(message_id) is even and messages are not added into the queue
        return get_error_response(msg='Testing error before even message!')

    logger.info(f'Will try to add {len(messages)} messages ...')
    for message_id, message in sorted(zip(message_ids, messages)):
        handle_message(message_id, message)

    # Error is returned if sum(message_id) is even after messages were added into the queue
    status_code = 500 if return_error_after_even_message and sum(message_ids) % 2 == 0 else 200

    return get_response(status=ResponseStatus.ok, status_code=status_code)


def run_app() -> None:
    global post_delay, return_error_before_even_message, return_error_after_even_message, logger

    host = os.environ['SECONDARY_HOST']
    port = int(os.environ['SECONDARY_PORT'])
    post_delay = int(os.getenv('POST_DELAY', 0))
    return_error_before_even_message = bool(os.getenv('ERROR_BEFORE_EVEN_MESSAGE', 'false') == 'true')
    return_error_after_even_message = bool(os.getenv('ERROR_AFTER_EVEN_MESSAGE', 'false') == 'true')

    app.run(host=host, port=port, debug=False)


def main() -> None:
    global logger

    logger = get_console_logger(service_name, level=logging.DEBUG)
    run_app()


if __name__ == '__main__':
    main()
