using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class MoveToSound : MonoBehaviour
{
    public FMODUnity.EventReference eventRef;
    public int window_size;
    public FMOD.DSP_FFT_WINDOW window_type = FMOD.DSP_FFT_WINDOW.RECT;
    
    private FMOD.Studio.EventInstance instance;
    private FMOD.ChannelGroup channelGroup;
    private FMOD.DSP dsp;
    private FMOD.DSP_PARAMETER_FFT fft_params;

    public float[] samples;

    public uint band;
    
    // Start is called before the first frame update
    void Start()
    {
        PrepareFMODEventInstance();
        
        samples = new float[window_size];
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumData();
        TransformToSound();
    }

    private void PrepareFMODEventInstance()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(eventRef);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        instance.start();
        
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out dsp);
        dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)window_type);
        dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, window_size * 2);

        instance.getChannelGroup(out channelGroup);
        channelGroup.addDSP(0, dsp);
    }

    private void GetSpectrumData()
    {
        System.IntPtr data;
        uint length;
        
        dsp.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out data, out length);
        fft_params = (FMOD.DSP_PARAMETER_FFT)Marshal.PtrToStructure(data, typeof(FMOD.DSP_PARAMETER_FFT));

        if (fft_params.numchannels == 0)
        {
            instance.getChannelGroup(out channelGroup);
            channelGroup.addDSP(0, dsp);
        } else if (fft_params.numchannels >= 1)
        {
            for (int b = 0; b < window_size; b++)
            {
                float total_chanel_data = 0f;
                for (int c = 0; c < fft_params.numchannels; c++)
                {
                    total_chanel_data += fft_params.spectrum[c][b];
                }
                samples[b] = total_chanel_data / fft_params.numchannels;
            }
        }
    }

    private void TransformToSound()
    {
        float val = Mathf.Lerp(transform.localScale.y, samples[band], 0.7f);
        transform.localScale = new Vector3(1, val * 3f, 1);
    }
}
