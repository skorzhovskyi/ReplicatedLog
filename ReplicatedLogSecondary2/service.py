
import logging
import os
import time
from queue import Queue

import flask

from utils import get_console_logger


app = flask.Flask(__name__)
logger = get_console_logger('replicated-log-secondary-2', level=logging.DEBUG)

messages = Queue()

# Delay in sec for POST requests, default is no delay
post_delay = int(os.getenv('POST_DELAY', 0))


def _get_error_response(msg: str) -> flask.Response:
    """Log error response with stacktrace before return"""
    logger.exception(msg)
    return flask.jsonify({'status': 'error', 'error': msg})


@app.route("/", methods=['GET'])
def get_messages():
    logger.info('Get all messages ...')
    response = {'status': 'ok', 'messages': [message for message in messages.queue]}
    logger.info(f'There are {messages.qsize()} messages in queue.')

    return flask.jsonify(response)


@app.route("/", methods=['POST'])
def append_message():

    try:
        request_body = flask.request.get_json()
    except TypeError:
        return _get_error_response('Could not parse input json!')

    try:
        message = request_body['message']
    except KeyError:
        return _get_error_response('Not found "message" in input json!')
    except TypeError:
        return _get_error_response(
            'Could not get data from request! '
            'Possible reasons: missing Content-Type in headers.'
        )

    if post_delay > 0:
        logger.debug(f'Sleep for {post_delay} seconds ...')
        time.sleep(post_delay)

    logger.info(f'Append [message="{message}"] ...')
    messages.put(message)
    logger.info(f'There are {messages.qsize()} messages in queue.')

    return flask.jsonify({'status': 'ok'})


def main():
    host = os.environ['SECONDARY_HOST']
    port = int(os.environ['SECONDARY_PORT'])
    app.run(host=host, port=port, debug=False)


if __name__ == '__main__':
    main()
