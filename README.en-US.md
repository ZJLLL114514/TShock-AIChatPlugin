# AIChatPlugin - Terraria Plugin

## Overview
AIChatPlugin is a plugin designed for Terraria game servers, allowing players to interact with an AI dialogue system via chat. The plugin provides a simple interface for players to ask questions to the AI using specific commands or trigger words and receive responses.

## Features
- **Command Triggering**: Players can activate AI dialogue using commands or custom trigger words in chat.
- **Context Management**: The plugin keeps track of players' dialogue context to provide more coherent responses.
- **Flexible Configuration**: Administrators can customize the AI's behavior through a configuration file, including trigger words and response length limits.
- **Help Command**: An integrated help command assists players in understanding how to use the plugin.
- **Reset Functionality**: Players can reset their dialogue context using a command.
- **Online Search**: Depending on the configuration, the AI can enable online search functionality to enhance answers.
- **Message Forwarding**: Supports forwarding messages to specified QQ group chats.

## Installation Steps
1. Place the AIChatPlugin files in the ServerPlugins directory.
2. Ensure your Terraria server has TShock installed and running.
3. Restart TShock to load the plugin.

## Usage
- Activate AI Dialogue: Type AI or other custom trigger words in the chat, followed by your question.
- Send Commands:
  - `/ab`: Ask AI a question.
  - `/bcz`: Clear your context history.
  - `/bbz`: Show help information.
  - `/aiclearall`: Admin command to clear all players' context records.

## Configuration File
AIChatPlugin uses a JSON configuration file located in the TShock directory, named AIChatConfig.json. You can edit this file to customize the plugin's behavior:

- **Model Selection**: Choose the AI model (1 for general, 2 for speed).
- **Trigger Words**: Define keywords to trigger AI dialogue.
- **Response Length Limit**: Set the maximum number of words for AI responses.
- **Line Break Count**: Set the number of characters after which AI answers will auto-wrap.
- **Context Limit**: Set the maximum number of context records for each player.
- **Online Search**: Decide whether the AI can perform online searches.
- **QQ Forwarding Switch**: Enable message forwarding to QQ group chats.
- **Forward QQ Group**: Specify the QQ group number for forwarding messages.
- **Settings**: Customize AI behavior settings.
- **Temperature**: Set the temperature parameter for AI responses.
- **Top_p Sampling**: Set the top_p parameter for AI responses.

## Notes
- Ensure your server has a stable internet connection for the AI to access necessary APIs.
- If you do not wish to use the message forwarding feature, ensure relevant configuration options are set to disabled.

## Contributions and Support
For any issues or suggestions, feel free to submit issues or pull requests on GitHub. You can also contact the plugin author: Mirror Qiluban.
