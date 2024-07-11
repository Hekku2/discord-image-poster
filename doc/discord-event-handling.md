# Discord event handling / commands

Things that need to be done before this App can be commanded from Discord:

 1. Application needs to be up and running!
 2. `INTERACTIONS ENDPOINT URL` needs to be configured, this requires an
 endpoint which can handle `PING` from Discord. See
 [Discord API Documentation](https://discord.com/developers/docs/interactions/overview#preparing-for-interactions) for details.
 3. Command needs to be registed. This can be done with `ConsoleTester`.

We are using Webhooks / `INTERACTIONS ENDPOINT URL`, not the socket, because we
don't want to keep the Functions app running.

## Registering command

Slash command are given in Discord with `/<command-here>`, that triggers the webhook.

Example interaction from slash command

```json
{
    "type": 2,
    "token": "A_UNIQUE_TOKEN",
    "member": {
        "user": {
            "id": "53908232506183680",
            "username": "Mason",
            "avatar": "a_d5efa99b3eeaa7dd43acca82f5692432",
            "discriminator": "1337",
            "public_flags": 131141
        },
        "roles": ["539082325061836999"],
        "premium_since": null,
        "permissions": "2147483647",
        "pending": false,
        "nick": null,
        "mute": false,
        "joined_at": "2017-03-13T19:19:14.040000+00:00",
        "is_pending": false,
        "deaf": false
    },
    "id": "786008729715212338",
    "guild_id": "290926798626357999",
    "app_permissions": "442368",
    "guild_locale": "en-US",
    "locale": "en-US",
    "data": {
        "options": [{
            "type": 3,
            "name": "cardname",
            "value": "The Gitrog Monster"
        }],
        "type": 1,
        "name": "cardsearch",
        "id": "771825006014889984"
    },
    "channel_id": "645027906669510667"
}
```

## Links

 * [Slash command handling](https://discord.com/developers/docs/interactions/application-commands#slash-commands)
 * [Command service from Discord.Net](https://docs.discordnet.dev/api/Discord.Commands.CommandService.html)
