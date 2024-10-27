using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Echo_Controller : MonoBehaviour
{
    private AudioReverbFilter filter;

    [SerializeField] private BoxCollider Room1;
    [SerializeField] Audiofilter Room1_audiofilter;

    [SerializeField] private BoxCollider Room2;
    [SerializeField] Audiofilter Room2_audiofilter;

    [SerializeField] private BoxCollider kitchen;
    [SerializeField] Audiofilter kitchen_audiofilter;

    [SerializeField] private BoxCollider living_Room;
    [SerializeField] Audiofilter living_Room_audiofilter;

    [SerializeField] private BoxCollider toilet;
    [SerializeField] Audiofilter toilet_audiofilter;

    [SerializeField] private BoxCollider Hallway;
    [SerializeField] Audiofilter hallway_audiofilter;

    [SerializeField] private BoxCollider Studio;
    [SerializeField] Audiofilter Studio_audiofilter;

    public enum Audiofilter
    { Room, User, Generic , Cave, Dizzy, Hallway, Off}

    private AudioReverbPreset preset;
    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponent<AudioReverbFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void reset_preset_value()
    {
        filter.room = -6200.00f;
    }
    private AudioReverbPreset preset_return(Audiofilter audiofilter)
    {
        if(audiofilter == Audiofilter.Room)
        {
            return AudioReverbPreset.Room;
        }
        else if (audiofilter == Audiofilter.User)
        {
            
            return AudioReverbPreset.User;
        }
        else if (audiofilter == Audiofilter.Generic)
        {
            return AudioReverbPreset.Generic;
        }
        else if (audiofilter == Audiofilter.Cave)
        {
            return AudioReverbPreset.Cave;
        }
        else if (audiofilter == Audiofilter.Dizzy)
        {
            return AudioReverbPreset.Dizzy;
        }
        else if (audiofilter == Audiofilter.Hallway)
        {
            return AudioReverbPreset.Hallway;
        }
        else if (audiofilter == Audiofilter.Off)
        {
            return AudioReverbPreset.Off;

        }


        return AudioReverbPreset.Off;
    }
    private void OnTriggerEnter(Collider place_name)
    {
        
        if (place_name == Room1) 
        {
            filter.reverbPreset = preset_return(Room1_audiofilter);
            reset_preset_value();
        }
        else if(place_name == Room2)
        {
            filter.reverbPreset = preset_return(Room2_audiofilter);
            reset_preset_value();
        }
        else if(place_name == Hallway)
        {
            filter.reverbPreset = preset_return(hallway_audiofilter);
            reset_preset_value();
        }
        else if (place_name == toilet)
        {
            filter.reverbPreset = preset_return(toilet_audiofilter);
            reset_preset_value();
        }
        else if (place_name == living_Room)
        {
            filter.reverbPreset = preset_return(living_Room_audiofilter);
            reset_preset_value();
        }
        else if (place_name == kitchen)
        {
            filter.reverbPreset = preset_return(kitchen_audiofilter);
            reset_preset_value();
        }
        else if (place_name == Studio)
        {
            filter.reverbPreset = preset_return(Studio_audiofilter);
            reset_preset_value();
        }
        else
        {
            filter.reverbPreset = AudioReverbPreset.Off;
            reset_preset_value();
        }
    }
    
}
