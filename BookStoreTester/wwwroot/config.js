// config.js
const dotenv = require('dotenv');
dotenv.config();

module.exports = {
  openai: {
    apiKey: process.env.OPENAI_API_KEY,
    endpoint: process.env.OPENAI_ENDPOINT || 'https://api.openai.com/v1/chat/completions'
  },
  unsplash: {
    accessKey: process.env.UNSPLASH_ACCESS_KEY,
    endpoint: process.env.UNSPLASH_ENDPOINT || 'https://api.unsplash.com'
  }
};