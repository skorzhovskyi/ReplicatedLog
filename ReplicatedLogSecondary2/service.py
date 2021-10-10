import os
import sys
from queue import Queue

import flask


app = flask.Flask(__name__)
messages = Queue()


@app.route("/", methods=['GET'])
def get_messages():
    print('Get all messages ...', file=sys.stderr)
    response = {'messages': [message for message in messages.queue]}
    print(f'There are {messages.qsize()} messages in queue.', file=sys.stderr)

    return flask.jsonify(response)


@app.route("/", methods=['POST'])
def append_message():

    try:
        request_body = flask.request.get_json()
    except TypeError:
        return flask.jsonify({'status': 'error', 'error': 'Could not parse input json!'})

    try:
        message = request_body['message']
    except KeyError:
        return flask.jsonify({'status': 'error', 'error': 'Not found message in input json!'})

    print(f'Append [message="{message}"] ...', file=sys.stderr)
    messages.put(message)
    print(f'There are {messages.qsize()} messages in queue.', file=sys.stderr)

    return flask.jsonify({'status': 'ok'})


def main():
    host = os.environ['SLAVE_HOST']
    port = int(os.environ['SLAVE_PORT'])
    app.run(host=host, port=port, debug=True)


if __name__ == '__main__':
    main()
