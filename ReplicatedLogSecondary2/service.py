
import os
import typing as _t
from queue import Queue

import flask

from utils import get_console_logger


app = flask.Flask(__name__)
messages = Queue()
logger = get_console_logger('replicated-log-slave-2')


def _get_error_response(body: _t.Dict) -> flask.Response:
    """Log error response with stacktrace before return"""
    logger.exception(body)
    return flask.jsonify(body)


@app.route("/", methods=['GET'])
def get_messages():
    logger.info('Get all messages ...')
    response = {'messages': [message for message in messages.queue]}
    logger.info(f'There are {messages.qsize()} messages in queue.')

    return flask.jsonify(response)


@app.route("/", methods=['POST'])
def append_message():
    try:
        request_body = flask.request.get_json()
    except TypeError:
        return _get_error_response({'status': 'error', 'error': 'Could not parse input json!'})

    try:
        message = request_body['message']
    except KeyError:
        return _get_error_response({'status': 'error', 'error': 'Not found message in input json!'})

    logger.info(f'Append [message="{message}"] ...')
    messages.put(message)
    logger.info(f'There are {messages.qsize()} messages in queue.')

    return flask.jsonify({'status': 'ok'})


def main():
    host = os.environ['SLAVE_HOST']
    port = int(os.environ['SLAVE_PORT'])
    app.run(host=host, port=port, debug=False)


if __name__ == '__main__':
    main()
