const express = require('express');
const { Configuration, OpenAIApi } = require('openai');
const config = require('./config');

const app = express();
app.use(express.json());

// Secure review generation endpoint
app.post('/api/generate-review', async (req, res) => {
  try {
    const { language } = req.body;

    const openai = new OpenAIApi(new Configuration({
      apiKey: config.openai.apiKey
    }));

    const response = await openai.createChatCompletion({
      model: "gpt-4-turbo",
      messages: [{
        role: "user",
        content: `Generate a short book review (50 words) in ${language}`
      }]
    });

    res.json({ review: response.data.choices[0].message.content });
  } catch (error) {
    console.error("OpenAI error:", error);
    res.status(500).json({ error: "Failed to generate review" });
  }
});