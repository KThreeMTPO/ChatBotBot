# ChatBotBot

This is a project to try to give ChatGPT a robot body. 

Likely to start as a simple conversational device through a raspberry pi, but may continue to evolve over time by adding additional functionality.

# Projects

## MQTTServer

     A console application that hosts an MQQT server via MQTTNet: https://github.com/dotnet/MQTTnet/
     Messages are subscribed to by a self hosted client.
     A basic method of aggregating various sensor data is also contined in MessageAggregator.cs

## MQTTServer.Tests

    Unit Test Project for MQTTServer. 
    Tests mostly focus on message aggregator for now.
  
## RaspberryPiWebServer

    a .net web api that hosts a swagger enabled web service.  This may be deprecated or refactored into the MQTTServer at a later date.
    Currently it has endpoints for processing audio and returning Text. (with the hope to later route to ChatGPT)
    
### Prerequisites

    Whipser - https://github.com/openai/whisper
    DeepSpeech (to be deprecated) - https://github.com/mozilla/DeepSpeech


## AudioTest
    a simple console app that sends audio data to RaspberyPiWebServer, for testing.
    Will be deprecated by the raspberry pi robot code when it becomes a bit more flushed out



