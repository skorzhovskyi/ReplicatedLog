import os

from flask import Flask


app = Flask(__name__)


@app.route("/")
def hello_world():
    return "<p>Hello, World!</p>"


def main():
    host = os.environ['SLAVE_HOST']
    port = int(os.environ['SLAVE_PORT'])
    app.run(host=host, port=port)


if __name__ == '__main__':
    main()
