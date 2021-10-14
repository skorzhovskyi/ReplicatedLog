const app = require('express')();
const http = require('http').createServer(app);
const bodyParser = require('body-parser');
const port = 3000;
const DELAY = process.env.POST_DELAY;

app.use(bodyParser.urlencoded({extended: false}));
app.use(bodyParser.json());

var messages = [];

app.post('/', function(req, res) {
	setTimeout(function() {
		console.log(res.statusCode + ' Secondary_1 received: ' + req.body.message);
		messages.push(req.body.message);
		res.send(req.body.message);
	}, DELAY);
});

app.get('/', (req, res) => {
  console.log("All messages: " + messages);
  res.send({messages: messages});
})

http.listen(port, () => {
  console.log(`Server is listening on ${port}`);
})
