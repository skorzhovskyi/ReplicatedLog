const app = require('express')();
const http = require('http').createServer(app);
const io = require('socket.io')(http);
const bodyParser = require('body-parser');
const port = 3000;

app.use(bodyParser.urlencoded({extended: false}));
app.use(bodyParser.json());

const messages = [];

io.on('connection', (socket) => {

  app.get('/', (req, res) => {
    console.log('Client connected');
    res.send('Hello from Server!');
  })

  app.post('/', function(req, res) {
    console.log(res.statusCode + ' Received: ' + req.body.message);
    messages.push(req.body.message);
    res.send(req.body.message);

    io.emit('message', "All messages: " + messages);
  })

})


http.listen(port, () => {
  console.log(`Server is listening on ${port}`);
})
