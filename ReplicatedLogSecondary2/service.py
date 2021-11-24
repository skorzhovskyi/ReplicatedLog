
import logging
import os
import time
import typing as _t
from threading import RLock

import flask

from utils import get_console_logger


app = flask.Flask(__name__)
logger = get_console_logger('replicated-log-secondary-2', level=logging.DEBUG)

# Mapping from message id into message text, the insert order of messages is preserved
messages: _t.Dict[int, str] = {}
# Using lock on messages makes sure they are safe across multiple threads
# NOTE: Can be acquired multiple times by the same thread
messages_lock = RLock()

# Delay in sec for POST requests, default is no delay
post_delay = int(os.getenv('POST_DELAY', 0))


def get_response(status: str, msg: _t.Optional[str] = None, **kwargs) -> flask.Response:
    """Log error response with stacktrace before return"""
    response = dict(status=status, **kwargs)
    if msg is not None:
        if status == 'error':
            logger.exception(msg)
        response['message'] = msg
    # TODO: Print endpoint and method
    logger.debug(f'Response: {response}')

    return flask.jsonify(response)


def get_error_response(msg: str) -> flask.Response:
    return get_response(status='error', msg=msg)


@app.route("/", methods=['GET'])
def get_messages():
    logger.info('Get all messages ...')
    return get_response(status='ok', messages=list(messages.values()))


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
        return get_error_response(
            'Could not get data from request! '
            'Possible reasons: missing Content-Type in headers.'
        )

    try:
        message_id = int(request_body['id'])
    except KeyError:
        return get_error_response('Not found field "id" inside input json!')
    except ValueError:
        return get_error_response('Could not parse field "id" from input json!')

    if post_delay > 0:
        logger.debug(f'Sleep for {post_delay} seconds ...')
        time.sleep(post_delay)

    with messages_lock:

        # Deduplication
        if message_id in messages:
            logger.info(f'[id={message_id}] Message with such id already exists!')
            return get_response(status='already_exists', message_id=message_id)

        logger.info(f'[id={message_id}] Add new message ...')
        messages[message_id] = message
        logger.info(f'There are {len(messages)} messages in queue.')

    return flask.jsonify({'status': 'ok'})


def main():
    host = os.environ['SECONDARY_HOST']
    port = int(os.environ['SECONDARY_PORT'])
    app.run(host=host, port=port, debug=False)


if __name__ == '__main__':
    main()
