const app = require('express')();
const http = require('http').createServer(app);
const bodyParser = require('body-parser');
const port = 3000;
const POST_DELAY_MS = process.env.POST_DELAY * 1000;

app.use(bodyParser.urlencoded({extended: false}));
app.use(bodyParser.json());

var messages = [];
var queue = [];

function callDelay() {
  return new Promise(resolve => {
    setTimeout(() => {
      resolve('Secondary_1 responded');
    }, POST_DELAY_MS);
  });
}

app.post('/', async (req, res) => {
	await callDelay();
		if(messages.length > 0) {
			var result = messages.find(({id}) => id == req.body.id);
			if(result == undefined) {
				var result2 = messages.find(({id}) => id == req.body.id - 1);
				if(result2 != undefined) {
					console.log(res.statusCode + ' Secondary_1 received: ' + req.body.id + ' > ' + req.body.message);
					messages.push(req.body);
				} else {
					var result = queue.find(({id}) => id == req.body.id);
					if(result == undefined) {
						console.log(res.statusCode + ' Secondary_1 has in queue: ' + req.body.id + ' > ' + req.body.message);
						queue.push(req.body);
					}	else {
						console.log(res.statusCode + ' Secondary_1 rejected duplication. Message is in queue with id: ' + req.body.id);
					}
				}
			} else {
				console.log(res.statusCode + ' Secondary_1 rejected duplication. Message already exists with id: ' + req.body.id);
			}
		} else {
			if(req.body.id == 1) {
				console.log(res.statusCode + ' Secondary_1 received: ' + req.body.id + ' > ' + req.body.message);
				messages.push(req.body);
			} else {
				var result2 = messages.find(({id}) => id == req.body.id - 1);
				if(result2 != undefined) {
					console.log(res.statusCode + ' Secondary_1 received: ' + req.body.id + ' > ' + req.body.message);
					messages.push(req.body);
				} else {
					if(queue.length > 0) {
						var result3 = queue.find(({id}) => id == req.body.id);
						if(result3 == undefined) {
							console.log(res.statusCode + ' Secondary_1 has in queue: ' + req.body.id + ' > ' + req.body.message);
							queue.push(req.body);
						} else {
							console.log(res.statusCode + ' Secondary_1 rejected duplication. Message is in queue with id: ' + req.body.id);
						}
					} else {
						console.log(res.statusCode + ' Secondary_1 has in queue: ' + req.body.id + ' > ' + req.body.message);
						queue.push(req.body);
					}
				}
			}
		}
	res.send(req.body);
});

app.get('/', async (req, res) => {
	var ordered = messages.sort(function(a, b) {
		if(a.id !== b.id) {
			return a.id - b.id
		}
	});

	var ordered2 = queue.sort(function(a, b) {
		if(a.id !== b.id){
			return a.id - b.id
		}
	});

  if(ordered.length != 0 && ordered2.length != 0){
    if(ordered[ordered.length - 1].id == ordered2[0].id - 1) {
  			ordered.push(ordered2[0]);
  			ordered2.shift();
  	}
  }

	console.log("Secondary_1 All messages: " + JSON.stringify(ordered));
	console.log("Secondary_1 Messages in queue: " + JSON.stringify(ordered2));
  res.send({messages: ordered});
})

app.get('/health', async (req, res) => {
	console.log(res.statusCode + " Secondary_1 is alive");
  res.send();
})

http.listen(port, () => {
  console.log(`Server is listening on ${port}`);
});
