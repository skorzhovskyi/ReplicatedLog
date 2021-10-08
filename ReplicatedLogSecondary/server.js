const app = require('express')();
const http = require('http').createServer(app);
const bodyParser = require('body-parser');
const port = 3000;

app.use(bodyParser.urlencoded({extended: false}));
app.use(bodyParser.json());

const messages = [];

app.post('/', function(req, res) {
  console.log(res.statusCode + ' Received: ' + req.body.message);
  messages.push(req.body.message);
  res.send(req.body.message);
})

app.get('/', (req, res) => {
  console.log("All messages: " + messages);
  res.send("All messages: " + messages);
})

http.listen(port, () => {
  console.log(`Server is listening on ${port}`);
})
