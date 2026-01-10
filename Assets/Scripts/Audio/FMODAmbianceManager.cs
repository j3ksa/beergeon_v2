using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAmbianceManager : MonoBehaviour
{
    [Header("Select Ambiance Events From FMOD")]
    public EventReference[] ambianceEvents; // This holds the different ambiance event that we wish to use from FMOD.
    [Header("Enter The Parameter Name To Control")]
    public string volCtrlParam; // This holds the name of the FMOD parameter that can control the volume of our ambiances.
    [Header("Play An Event When Game Starts")]
    public bool playOnStart = true; // Do you want to play an ambiance as soon as the game starts?
    public int EventNumber = 0; // If you do, which ambiance is it? Enter the ambiances element number here to select it.
    public float startVolCtrlParamAt; // What number does your Volume Control Parameter need to start at for your ambiance to be heard?
    public enum EventCreation { AllOnStart, OnTheFly };
    [Header("When Will Event Instances Be Created?")]
    public EventCreation createEvents; // Do you want ot create instances of your amabiances as soon as the game starts or as they're needed?

    private EventInstance[] ambEventInstances; // This will hold all instances of our ambiances.
    private int chosenAmbiance; // This is used to select and represent a specific ambiance from our collection of ambiance events.
    private EventDescription eventInfo; // This will be used to hold info about one of our ambiance events.
    private PARAMETER_DESCRIPTION parameterInfo; // This will later be used to hold info about our Volume Control Parameter.
    private float paramMin; // This will hold the minimum value that the Volume Control Parameter can be set to.
    private float paramMax; // This will hold the maximum value that the Volume Control Parameter can be set to.
    private float paramRange; // This will hold the total range betweent the min and max values of the Volume Control Parameter.

    private void Start() // Everything in this function happens once as soon as the game starts.
    {
        ambEventInstances = new EventInstance[ambianceEvents.Length]; // Depending on how many ambiance events we have (Length) then we'll create a new empty array of that size to hold them in.

        if (createEvents == EventCreation.AllOnStart) // If we chose to create all of our event instances as soon as the game starts...
        {
            for (int i = 0; i < ambianceEvents.Length; i++) // ... then for each empty slot in our array (ambEventInstances) ...
                ambEventInstances[i] = RuntimeManager.CreateInstance(ambianceEvents[i]); // ... create a new instance of each amabince event we have and store them in the empty array slots.
            if (playOnStart) // If we want to play an event staright away...
            {
                ambEventInstances[EventNumber].start(); // Then play the one we chose the event number for.
                ambEventInstances[EventNumber].setParameterByName(volCtrlParam, startVolCtrlParamAt);
            }
        }
        else // If we chose to create ambiance instances on the fly then we don't have to create any yet.
        {
            if (playOnStart) // If we want to play an ambiance now however, then we do need to create a single instance for that event.
            {
                ambEventInstances[EventNumber] = RuntimeManager.CreateInstance(ambianceEvents[EventNumber]);
                ambEventInstances[EventNumber].start();
                ambEventInstances[EventNumber].setParameterByName(volCtrlParam, startVolCtrlParamAt);
            }
        }

        // Here is here we look at our Volume Contorl Parmater and find out what it's min value, max value and range is.
        // This is so we know by how much we need to increase and decrease the parameter by as the player travels through the trigger box.
        eventInfo = RuntimeManager.GetEventDescription(ambianceEvents[0]); // First take an ambiance event, use 'GetEventDescription' to get info about it and store it in 'eventInfo'.
        eventInfo.getParameterDescriptionByName(volCtrlParam, out parameterInfo); // We can then access the Parameter through 'eventInfo' and store it in 'parameterInfo'.
        paramMin = parameterInfo.minimum; // Then we can get each of those values from 'parameterInfo'.
        paramMax = parameterInfo.maximum;
        paramRange = paramMax - paramMin; // The difference between the min and max value is our range.
    }

    public void PlayAmbiance(int index) // This is called by the 'Trigger Box Audio Blender' script when the player enters the trigger box. 'index' the element number of the ambiance we want to play.
    {
        if (createEvents == EventCreation.OnTheFly) // If we're creating instances on the fly...
            ambEventInstances[index] = RuntimeManager.CreateInstance(ambianceEvents[index]); // ...then create an instance of the ambiance we wish to play.
        ambEventInstances[index].start(); // Play the ambiance.
    }

    public void SelectAmbiance(int index) // 'chosenAmbiance' holds the element number of the ambiance that was selected by the person who set up the 'Trigger Box Audio Blender' script that called this function.
    {
        chosenAmbiance = index;
    }

    public void IncreaseParamViaAtoB(TriggerBoxAudioBlender triggerBoxAudioBlender) // Increase the Volume Control Parameter's value for the 'chosenAmbiance' based on the players progress through the trigger box.
    {
        float newParamValue = (triggerBoxAudioBlender.ProgressThroughTrigger / 100f * paramRange) + paramMin;
        ambEventInstances[chosenAmbiance].setParameterByName(volCtrlParam, newParamValue);
    }

    public void DecreaseParamViaAtoB(TriggerBoxAudioBlender triggerBoxAudioBlender) // Decrease the Volume Control Parameter's value for the 'chosenAmbiance' based on the players progress through the trigger box.
    {
        float newParamValue = paramMax - (triggerBoxAudioBlender.ProgressThroughTrigger / 100f * paramRange);
        ambEventInstances[chosenAmbiance].setParameterByName(volCtrlParam, newParamValue);
    }

    public void StopAmbiance(int index) // Tell the amabince with the elemtn number that matches 'index' to stop.
    {
        ambEventInstances[index].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        if (createEvents == EventCreation.OnTheFly)
            ambEventInstances[index].release();
    }

    private void OnDestroy() // If the gameObject that this script is attached to is destroyed, then this funciton stops any amabince playing and destroys all of our created ambiance event instances.
    {
        for (int i = 0; i < ambianceEvents.Length; i++)
        {
            StopAmbiance(i);
            ambEventInstances[i].release();
        }
    }
}