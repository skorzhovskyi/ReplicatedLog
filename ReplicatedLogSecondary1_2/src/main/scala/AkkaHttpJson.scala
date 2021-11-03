import akka.actor.typed.ActorSystem
import akka.actor.typed.scaladsl.Behaviors
import akka.http.scaladsl.Http
import akka.http.scaladsl.marshallers.sprayjson.SprayJsonSupport
import akka.http.scaladsl.model._
import akka.http.scaladsl.server.Directives.{post, _}
import akka.http.scaladsl.server.Route
import spray.json._
import scala.collection.mutable.ListBuffer

case class Message(message: String)
case class Messages(messages: List[String])

trait MessageJsonProtocol extends DefaultJsonProtocol {
  implicit val messageFormat = jsonFormat1(Message)
  implicit val messagesFormat = jsonFormat1(Messages)
}

object AkkaHttpJson extends MessageJsonProtocol with SprayJsonSupport {
  implicit val system = ActorSystem(Behaviors.empty, "AkkaHttpJson")
  val messages = new ListBuffer[String]

  def createActor() = Behaviors.receiveMessage[String] { someMessage =>
    Thread.sleep(10000L)
    messages += someMessage
    println(HttpResponse.apply().status + s" Scala server received a message: $someMessage")
    Behaviors.same
  }
  val rootActor = ActorSystem(createActor(), "AkkaHttpJson")

  val route: Route = {
    (post & entity(as[Message])) {
      message: Message =>
        println(HttpResponse.apply().status + " Sent to Scala server: " + message.message)
        rootActor ! message.message
        complete {(Message(message.message))}
    }~
      (get) {
        println(HttpResponse.apply().status + " All messages from Scala server: " + messages.toList)
        complete {(Messages(messages.toList))}
      }
  }

  def main(args: Array[String]): Unit = {
    Http().newServerAt("localhost", 3000).bind(route)
    println("Scala server is listening on port 3000")
  }
}
