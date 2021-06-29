using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SpeechSDK
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;


public class AudioController : MonoBehaviour
{
    public String SubscKey, Region;
    public AudioSource audioSource;
    public String Language = "ja-JP";
    public String Speaker = "ja-JP-HarukaRUS";

    private SpeechConfig config;
    private SpeechSynthesizer synthesizer;

    // Start is called before the first frame update
    void Start()
    {
        this.config = SpeechConfig.FromSubscription(SubscKey, Region);
        this.config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw16Khz16BitMonoPcm);
        this.config.SpeechSynthesisLanguage = Language; // 日本語にする
        this.config.SpeechSynthesisVoiceName = Speaker; //読み上げ音声の設定
        this.synthesizer = new SpeechSynthesizer(this.config, null);
        //Task.Run(async () => { await SynthesizeAudioAsync(); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SynthesizeAudio(string text)
    {
        var config = SpeechConfig.FromSubscription(SubscKey, Region);
        config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw16Khz16BitMonoPcm);
        config.SpeechSynthesisLanguage = Language; // 日本語にする
        config.SpeechSynthesisVoiceName = Speaker; //読み上げ音声の設定
        var synthesizer = new SpeechSynthesizer(config, null);

        using (var result = synthesizer.SpeakTextAsync(text).Result)
        {
            Debug.Log("In Synthesize");
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                Debug.Log("Get Data");
                // 正常にデータがかえって来た。
                var sampleCount = result.AudioData.Length / 2;
                var audioData = new float[sampleCount];
                for (var i = 0; i < sampleCount; i++)
                {
                    audioData[i] = (short)(result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;
                }
                // 16K 16bit モノラルで出力
                try
                {
                    Debug.Log("Create Clip");
                    var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
                    Debug.Log("Set Data");
                    audioClip.SetData(audioData, 0);
                    audioSource.clip = audioClip;
                    Debug.Log("Play Sound");
                    audioSource.Play();
                    Debug.Log("Output: " + text);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error: " + e.ToString());
                }
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                // 何かしらの理由でキャンセルされた場合
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.LogError($"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?");
            }
        }
    }

    public void playWav(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    /*
    // 固定音声の生成に使用。後のために残しておく
    async Task SynthesizeAudioAsync()
    {
        using (var audioConfig = AudioConfig.FromWavFileOutput("file-name.wav"))
        {
            config = SpeechConfig.FromSubscription("Key", "Region");
            //config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw16Khz16BitMonoPcm);
            config.SpeechSynthesisLanguage = Language; // 日本語にする
            config.SpeechSynthesisVoiceName = Speaker; //読み上げ音声の設定

            //var audioConfig = AudioConfig.FromWavFileOutput("noBatch.wav");
            using (var synthesizer = new SpeechSynthesizer(config, audioConfig))
            {
                //var synthesizer = new SpeechSynthesizer(this.config, audioConfig);
                await synthesizer.SpeakTextAsync("読み上げテキスト");
                Debug.Log("確認用のログ");
            }
        }
    }
    */
}
