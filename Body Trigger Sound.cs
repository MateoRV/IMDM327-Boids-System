using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderData;

public class BodyTriggerSound : MonoBehaviour
{
    //public AudioSource bass;
    Dictionary<string, int> notes;
    public int noteDelay;
    float timer;
    Boolean playNote = false;
    float noteLength = .3f;
    float noteLengthCounter = 0;
    System.Random rand = new System.Random();
    float timeIdx = 0;

    [Range(100f, 3000f)]  //Creates a slider in the inspector

    public float tempo;


    [Range(0f, 10f)]  //Creates a slider in the inspector

    public float carMul;

    [Range(0f, 10f)]  //Creates a slider in the inspector

    public float modMul;

    AudioSource audioSource;

    [Range(20, 20000)]  //Creates a slider in the inspector
    public float frequency;
    public float sampleRate = 44100f;
    [Range(0.1f, 2)]  //Creates a slider in the inspector
    public float amplitude;
    float phase = 0; // phase of an oscillator. If many, this also should be an array

    // Start is called before the first frame update
    void Start()
    {
        //bass.Play();
        //Initialize an array of all frequencies of notes in the key
        //Have a timer that will randomly trigger the noise
        //Ie. Random.Range(index 0, index n -1)
        //Dictionary (Hash Map) of the key of G major
        notes = new Dictionary<string, int>();
        //(The frequency of all the notes is one half step lower than they should be. whatever audio script we're using is
        //slightly off key
        notes.Add("G1", 185);
        notes.Add("G", 370);
        notes.Add("B", 466);
        notes.Add("C", 493);
        notes.Add("D", 554);
        notes.Add("E", 622);
        notes.Add("G2", 740);
        //Debug.Log(notes["A"]);
        timer = (float) (noteDelay * rand.NextDouble());

        audioSource = gameObject.AddComponent<AudioSource>();
        //frequency = 440f; // inital frequency
        amplitude = 0.3f; // inital amplitude
        tempo = 1000;

        carMul = 1; modMul = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= noteDelay) // trigger of sound synth
        {
            //Random note
            List<string> keyList = new List<string>(notes.Keys);

            string randomKey = keyList[rand.Next(keyList.Count)];
            Debug.Log(randomKey);
            frequency = notes[randomKey];

            playNote = true;
            if (!audioSource.isPlaying)
            {
                timeIdx = 0;  //resets timer before playing sound
                audioSource.Play();
            }
            timer = 0;
        }
        // turn off the audio when the envelope is small enough.
        if (timeIdx > 1000 && Envelope(timeIdx, tempo) < 0.001)
        {
            audioSource.Stop();
            timeIdx = 0;
            playNote = !playNote;
        }
        timer += Time.deltaTime;
        /*        if (timer >= noteDelay)
                {

                    //play a random note
                    List<string> keyList = new List<string>(notes.Keys);

                    string randomKey = keyList[rand.Next(keyList.Count)];
                    frequency = notes[randomKey];
                    Debug.Log(randomKey);

                    playNote = true;
                    timer = 0;
                }

                if (playNote)
                {
                    noteLengthCounter += Time.deltaTime;
                    if (noteLengthCounter >= noteLength)
                    {
                        playNote = false;
                        noteLengthCounter = 0;
                    }
                }
                timer += Time.deltaTime;*/

        //Calculate vector to the camera object, if it is within a certain range the body is "active"
        //Active bodies trigger their sound randomly once every 10 seconds maybe idk
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (playNote)
        {
            for (int i = 0; i < data.Length; i += channels)
            {
                //frequency and phase. Phase is like number of times the wave goes through 0 in a second. So if the
                //frequency is 1, then phase is 2.
                phase += 2 * Mathf.PI * frequency / sampleRate;
                data[i] = amplitude * FM(phase, carMul, modMul) * Envelope(timeIdx, tempo);
                data[i + 1] = data[i]; // for now, right channel = left channel
                if (phase >= 2 * Mathf.PI)
                {
                    phase -= 2 * Mathf.PI;
                }
                timeIdx++;
            }

        }

    }


    public float SinWave(float phase)

    {
        return Mathf.Sin(phase);
    }
    public float FM(float phase, float carMul, float modMul)

    {

        return Mathf.Sin(phase * carMul) + Mathf.Sin(phase * modMul); // fluctuating FM

    }

    public float Envelope(float timeIdx, float tempo)

    {   // should have something looks like..: /\__

        // https://www.sciencedirect.com/topics/engineering/envelope-function

        float a = 0.13f;

        float b = 0.45f;

        return Mathf.Abs(Mathf.Exp(-a * (timeIdx) / tempo) - Mathf.Exp(-b * (timeIdx) / tempo));

    }
}
