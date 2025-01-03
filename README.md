# AIChatPlugin - Terraria 插件

## 概述

AIChatPlugin 是一个为 Terraria 游戏服务器设计的插件，它允许玩家通过聊天与一个 AI 对话系统进行互动。该插件提供了一个简单的接口，使得玩家可以通过特定的命令或聊天触发词来向 AI 提出问题，并接收回答。

## 功能特点

- **命令触发**：玩家可以通过命令或聊天触发词来激活 AI 对话。
- **上下文管理**：插件会记录玩家的对话上下文，以便提供更连贯的回答。
- **配置灵活**：通过配置文件，管理员可以自定义 AI 的行为，包括触发词、回答字数限制等。
- **帮助命令**：内置的帮助命令可以帮助玩家了解如何使用插件。
- **重置功能**：玩家可以通过命令重置自己的对话上下文。
- **联网搜索**：根据配置，AI 可以启用联网搜索功能来增强回答。
- **消息转发**：支持将消息转发到指定的 QQ 群聊。

## 安装步骤

1. 将 AIChatPlugin 插件文件放置在 ServerPlugins 插件目录下。
2. 确保你的 Terraria 服务器已经安装并运行 TShock。
3. 重启 TShock 以加载插件。

## 使用方法

- **激活 AI 对话**：在聊天栏输入 `AI` 或其他自定义的触发词，然后输入你的问题。
- **发送命令**：
  - `/aibot` 或 `/ab`：向 AI 提问。
  - `/bot重置` 或 `/bcz`：清除你的上下文记录。
  - `/bot帮助` 或 `/bbz`：显示帮助信息。
  - `/清除所有人的上下文`：管理员命令，清除所有玩家的上下文记录。

## 配置文件

AIChatPlugin 使用一个 JSON 格式的配置文件，位于 `tshock` 目录下，名为 `AI聊天自定义配置.json`。你可以编辑此文件来自定义插件的行为：

- **模型选择**：选择 AI 模型，1 为通用，2 为速度。
- **聊天触发 AI 提示词**：定义触发 AI 对话的关键词。
- **AI回答字数限制**：设置 AI 回答的最大字数。
- **AI回答换行字数**：设置 AI 回答自动换行的字数。
- **上下文限制**：设置每个玩家上下文记录的最大数量。
- **启用联网搜索**：是否允许 AI 进行联网搜索。
- **转发 QQ 群总开关**：是否启用消息转发到 QQ 群聊的功能。
- **转发 QQ 群**：指定要转发消息的 QQ 群号。
- **AI 设定**：自定义 AI 的行为设定。
- **temperature 温度**：设置 AI 回答的温度参数。
- **top_p 核采样**：设置 AI 回答的 top_p 参数。

## 注意事项

- 确保你的服务器网络连接正常，以便 AI 能够访问必要的 API。
- 如果遇到问题，检查配置文件是否正确配置，以及 API 密钥是否有效。
- 如果你不希望使用消息转发功能，请确保相关配置项为关闭状态。

## 贡献与支持

如果你有任何问题或建议，欢迎通过 GitHub 提交 issue 或 pull request。同时，你也可以联系插件作者：镜奇路蓝。

## English：

### AIChatPlugin - Terraria Plugin

## Overview

AIChatPlugin is a plugin for the Terraria game server that allows players to interact with an AI dialog system via chat. The plugin provides a simple interface that allows players to ask the AI questions and receive answers via specific commands or chat trigger words.

## Features

- **Command Trigger**: Players can activate AI conversations via commands or chat trigger words.
- **Context Management**: The plugin records the context of the player's conversations in order to provide more coherent answers.
- **Configuration Flexibility**: Through the configuration file, administrators can customize the behavior of the AI, including trigger words, answer word limits, etc.
- **Help Commands**: Built-in help commands help players understand how to use the plugin.
- **Reset Function**: Players can reset their dialog context through commands.
- **Networked Search**: Depending on the configuration, the AI can enable the networked search feature to enhance answers.
- **Message Forwarding**: Supports forwarding messages to specified QQ group chats.

## Installation Steps

1. Place the AIChatPlugin plugin file in the ServerPlugins plugin directory.
2. Ensure that your Terraria server has TShock installed and running. 3.
3. Restart TShock to load the plugin.

## Usage

- **Activate AI Conversation**: Type `AI` or another customized trigger word in the chat field and type your question.
- **Send command**:
  - `/aibot` or `/ab`: Ask the AI a question.
  - `/bot reset` or `/bcz`: clear your context record.
  - `/bot help` or `/bbz`: show help messages.
  - `/clear everyone's context`: administrator command that clears all players' context records.

## Configuration files

The AIChatPlugin uses a JSON-formatted configuration file, located in the `tshock` directory, called `AI Chat Custom Configuration.json`. You can edit this file to customize the behavior of the plugin:

- **Model Selection**: selects the AI model, 1 for generic, 2 for speed.
- **Chat Trigger AI Prompt Word**: define the keyword to trigger AI dialog.
- **AI Answer Word Limit**: set the maximum number of words for AI answer.
- **AI Answer Line Break Characters**: Set the number of characters for AI answer to automatically break lines.
- **Context Limit**: Set the maximum number of context records per player.
- **Enable Network Search**: Whether to allow AI to perform network search.
- **Forward QQ Groups General Switch**: whether to enable message forwarding to QQ group chat.
- **Forward QQ Groups**: Specify the QQ group number to forward messages to.
- **AI Settings**: customize the behavior of AI.
- **temperature**: set the temperature parameter for AI to answer.
- **top_p kernel sampling**: set the top_p parameter of AI response.

## Notes

- Make sure your server network connection is working so that the AI can access the necessary APIs.
- If you encounter problems, check that the configuration file is properly configured and that the API key is valid.
- If you do not wish to use the message forwarding feature, make sure the relevant configuration item is turned off.

## Contribute and support

If you have any questions or suggestions, please feel free to submit an issue or pull request via GitHub, and you can also contact the plugin's author, Mirrorchild Blue.

Translated with DeepL.com (free version)
