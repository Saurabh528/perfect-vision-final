from gtts import gTTS

position_names = {
    0: "top left",
    1: "top center",
    2: "top right",
    3: "middle left",
    4: "center",
    5: "middle right",
    6: "bottom left",
    7: "bottom center",
    8: "bottom right",
}

# Specify the language (optional, default is 'en')
language = 'en'
for dot_number in range(9):
    # The text you want to convert to speech
    text = f"Please look at the {position_names[dot_number]} dot"
    # Create a gTTS object
    tts = gTTS(text=text, lang=language, slow=False)

    # Save the audio file
    tts.save(f"{text}.mp3")

print("Text has been converted to speech and saved as output.mp3")