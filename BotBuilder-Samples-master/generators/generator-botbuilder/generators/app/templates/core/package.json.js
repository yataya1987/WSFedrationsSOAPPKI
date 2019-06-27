{
    "name": "<%= botname %>",
    "version": "1.0.0",
    "description": "<%= botDescription %>",
    "author": "Generated using Microsoft Bot Builder Yeoman generator v<%= version %>",
    "license": "MIT",
    "main": "<%= npmMain %>",
    "scripts": {
        "start": "node ./index.js",
        "watch": "nodemon ./index.js",
        "lint": "eslint .",
        "test": "echo \"Error: no test specified\" && exit 1"
    },
    "repository": {
        "type": "git",
        "url": "https://github.com"
    },
    "dependencies": {
        "@microsoft/recognizers-text-data-types-timex-expression": "1.1.4",
        "botbuilder": "~4.4.0",
        "botbuilder-ai": "~4.4.0",
        "botbuilder-dialogs": "~4.4.0",
        "dotenv": "^7.0.0",
        "replace": "~1.1.0",
        "restify": "~8.2.0"
    },
    "devDependencies": {
        "eslint": "^5.13.0",
        "eslint-config-standard": "^12.0.0",
        "eslint-plugin-import": "^2.16.0",
        "eslint-plugin-node": "^8.0.1",
        "eslint-plugin-promise": "^4.0.1",
        "eslint-plugin-standard": "^4.0.0",
        "nodemon": "^1.18.10"
    }
}
