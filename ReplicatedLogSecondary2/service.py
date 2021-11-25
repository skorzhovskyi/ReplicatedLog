
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
    already_exists = 'already_exists'


service_name = 'replicated-log-secondary-2'

app = flask.Flask(service_name)
logger: _t.Optional[logging.Logger] = None

# Mapping from message id into message text, the insert order of messages is preserved
messages: _t.Dict[int, str] = {}
# Using lock on messages makes sure they are safe across multiple threads
# NOTE: Can be acquired multiple times by the same thread
messages_lock = RLock()

# Delay in sec for POST requests, default is no delay
post_delay: _t.Optional[int] = None


def get_response(status: ResponseStatus, msg: _t.Optional[str] = None, **kwargs) -> flask.Response:
    """Log error response with stacktrace before return"""
    response = dict(status=status.value, **kwargs)
    if msg is not None:
        if status == ResponseStatus.error:
            logger.exception(msg)
        response['message'] = msg

    # TODO: Print endpoint and method
    logger.debug(f'Response: {response}')

    return flask.jsonify(response)


def get_error_response(msg: str) -> flask.Response:
    return get_response(status=ResponseStatus.error, msg=msg)


def trim_messages() -> _t.Generator[str, None, None]:
    """Return messages till the end or the first missing"""
    with messages_lock:

        # Ordering
        # NOTE: Here unimportant, but this is not memory efficient (to create the copy of all messages)
        #       Using list for ready messages and dict for unready will be more efficient
        sorted_messages = sorted(messages.items())
        # NOTE: Python dict ensures, that messages are sorted as how they were inserted,
        #       and this may not be their natural order

        prev_message_id = None
        for message_id, message in sorted_messages:

            if prev_message_id is not None and message_id != prev_message_id + 1:
                logger.info(f'Stop at message with [id={message_id}]')
                break

            yield message

            prev_message_id = message_id


@app.route("/health", methods=['GET'])
def check_health():
    return get_response(status=ResponseStatus.ok)


@app.route("/", methods=['GET'])
def get_messages():
    logger.info('Get all messages ...')
    sorted_messages = list(trim_messages())
    logger.info(f'Found {len(sorted_messages)} messages.')

    return get_response(status=ResponseStatus.ok, messages=sorted_messages)


@app.route("/", methods=['POST'])
def append_message():

    try:
        request_body = flask.request.get_json()
    except TypeError:
        return get_error_response(msg='Could not parse input json!')

    try:
        message = request_body['message']
    except KeyError:
        return get_error_response('Not found "message" in input json!')
    except TypeError:
        return get_error_response('Could not get data from request! Possible reasons: missing Content-Type in headers')

    try:
        message_id = int(request_body['id'])
    except KeyError:
        return get_error_response('Not found field "id" inside input json!')
    except ValueError:
        return get_error_response('Could not parse field "id" from input json!')

    if post_delay > 0:
        logger.info(f'[id={message_id}] Sleep for {post_delay} seconds ...')
        time.sleep(post_delay)

    with messages_lock:

        # Deduplication
        if message_id in messages:
            logger.warning(f'[id={message_id}] Message with such id already exists!')
            return get_response(status=ResponseStatus.already_exists, message_id=message_id)

        logger.info(f'[id={message_id}] Add new message ...')
        messages[message_id] = message
        logger.info(f'There are {len(messages)} messages in queue.')

    return flask.jsonify({'status': 'ok'})


def run_app() -> None:
    global post_delay, logger

    host = os.environ['SECONDARY_HOST']
    port = int(os.environ['SECONDARY_PORT'])
    post_delay = int(os.getenv('POST_DELAY', 0))

    app.run(host=host, port=port, debug=False)


def main() -> None:
    global logger

    logger = get_console_logger(service_name, level=logging.DEBUG)
    run_app()


if __name__ == '__main__':
    main()
