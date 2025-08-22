# VK Console Messenger

A console-based VKontakte messenger that works by simulating browser HTTP requests instead of using the official API.

## About the Program

This is a lightweight console application that allows you to access VKontakte messaging functionality directly from the command line. The application works by mimicking browser HTTP requests, providing a text-based interface for managing your VK conversations and messages.

## Features

- **Dialog Management**: View and navigate through your conversation list
- **Message History**: Access message history with pagination support
- **Send Messages**: Send text messages to any conversation
- **File Downloads**: Download documents and files from conversations
- **Persistent Sessions**: Save login data to avoid re-authentication
- **Browser Simulation**: Works without official API by mimicking browser requests

## Installation

1. Download the program archive and extract it to any folder on your computer.
2. Run **vk-console.exe** in the extracted folder.
3. Enter your VKontakte login and password when prompted.
4. Start using the VKontakte console messenger!

| [Download vk-console](https://github.com/nayutalienx/vk-console-not-api/releases/download/0.0/Release.rar) |
| ------------- |

## Available Commands

    dialogs                    [get list of dialogs/conversations]
    moreDialogs               [get older dialogs]
    !moreDialogs              [get newer dialogs]
    First Name Last Name      [get messages from a specific dialog]
    more                      [get older messages from current dialog]
    ~message text             [send message to the last visited dialog]
    doc Document_name.ext     [download document to docs folder]
    reset                     [logout from account]
    exit                      [exit and SAVE ALL DATA, otherwise you'll need to re-enter everything]

## Usage Instructions

1. **Initial Setup**: On first run, enter your VKontakte credentials
2. **View Conversations**: Use `dialogs` to see your conversation list
3. **Open a Chat**: Type the contact's name (e.g., "John Smith") to open their conversation
4. **Send Messages**: Use `~Hello there!` to send a message to the currently open conversation
5. **Navigate History**: Use `more` and `moreDialogs` to browse through older content
6. **Download Files**: Use `doc filename.ext` to download documents from conversations
7. **Session Management**: Use `exit` to properly save your session, or `reset` to logout

## Technical Details

- **Language**: C# (.NET Framework)
- **Authentication**: Uses standard VKontakte web authentication
- **Data Storage**: Local file-based storage for session persistence
- **Network**: HTTP requests with proper headers and cookie management
- **UI**: Console-based interface with colored output

## Requirements

- Windows operating system
- .NET Framework
- Internet connection
- Valid VKontakte account

## Security Notice

This application stores authentication data locally. Make sure to use `exit` command to properly save your session and keep your computer secure.

