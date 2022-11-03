# AI Discord Friend
A Discord bot that can respond to messages in servers and in people DMs.
It has active and inactive periods where is changes it status to online and
idle respectively. When it is inactive it will still respond to DMs but won't
respond to messages in servers.

## Technologies
This project was created using C# and the Discord.Net library.
The AI part of the bot is done with OpenAI's GPT-3 API.

## Safety
### Collection of data
When the bot it asked to generate a response it will collect the last 5
messages from that channel and sends them to OpenAI and also saves them 
to a file for future use. The only message information saved is the message
itself and the author's username. The hashed userid of the person the bot 
is replying to will also be sent to OpenAI so they can moderate.

### Filter
The bot users OpenAI's moderation API to attempt to filter out anything 
that would break OpenAI's terms of service.
However, the moderation is not perfect in any way and explicit for offensive 
content can still be generated.

## Setup

### Config
All the configuration is done in the `config.json` file.  

The `token` field is the bot's Discord token.  

The `open_ai_token` field is the OpenAI API token which can be 
obtained on OpenAI's website.  
The `active_period_length_minutes` field is how long on average the bot's
active period should last.  

The `active_period_length_variation` field is how much the active period
should vary. (Calculated with `Active Period Length = 
active_period_length_minutes + random(-active_period_length_variation, 
active_period_length_variation)`)  

The `down_time_length_minutes` field is how long on average the bot's
inactive period should last.  

The `down_time_length_variation` field is how much the inactive period
should vary. (Calculated with `Inactive Period Length =
down_time_length_minutes + random(-down_time_length_variation,
down_time_length_variation)`)  

The `messages_per_hour_max` field is the maximum number of messages the bot
will reply to per hour. (If number of messages sent in the last hour goes 
above this number then the bot will ignore the message)  

The `open_ai_model` field is the model that OpenAI will use to generate the
response. (Recommended: `text-davinci-002`)

### Run
All you need to do is run the bot and it will work.