﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SheetMusicCycle : MonoBehaviour {

    static float BETWEEN_STAFF_LINE_DISTANCE = .01f;
    static float LINE_OFFSET = .01f;

    static float BETWEEN_STAFF_LINE_DISTANCE_small = .00625f;
    static float LINE_OFFSET_small = .00625f;

    static float ERROR_KEY_DURATION = 1;

    const float TOLERANCE = .5f;

    Song song;
    GameObject playhead;
    Dictionary<int, float> ErrorKeys;

    #region notes
    public Transform quarter_note;
    public Transform whole_note;
    public Transform half_note;
    public Transform eighth_note;
    public Transform sixteenth_note;
    public Transform dotted_quarter_note;
    public Transform dotted_whole_note;
    public Transform dotted_half_note;
    public Transform dotted_eighth_note;
    public Transform dotted_sixteenth_note;
    public Transform quarter_note_small;
    public Transform whole_note_small;
    public Transform half_note_small;
    public Transform eighth_note_small;
    public Transform sixteenth_note_small;
    public Transform dotted_quarter_note_small;
    public Transform dotted_whole_note_small;
    public Transform dotted_half_note_small;
    public Transform dotted_eighth_note_small;
    public Transform dotted_sixteenth_note_small;

    public Transform quarter_rest;
    public Transform whole_rest;
    public Transform half_rest;
    public Transform eighth_rest;
    public Transform quarter_rest_small;
    public Transform whole_rest_small;
    public Transform half_rest_small;
    public Transform eighth_rest_small;

    #endregion

    #region staff_stuff
    public Transform ledger_line;
    public Transform ledger_line_small;

    GameObject[] MusicSheet = new GameObject[4];

    GameObject[] StaffLine1 = new GameObject[4];
    GameObject[] StaffLine2 = new GameObject[4];
    GameObject[] StaffLine3 = new GameObject[4];
    GameObject[] StaffLine4 = new GameObject[4];
    GameObject[] StaffLine5 = new GameObject[4];
    GameObject[] StaffLine6 = new GameObject[4];
    GameObject[] StaffLine7 = new GameObject[4];
    GameObject[] StaffLine8 = new GameObject[4];
    GameObject[] StaffLine9 = new GameObject[4];
    GameObject[] StaffLine10 = new GameObject[4];
    #endregion

    int CurrentBar;
    int LastBar = 0;

    Bar bar;
    Bar bar_prev1;
    Bar bar_next1;
    Bar bar_next2;


    //EVENT MANAGEMENT:
    #region event_management
    ArrayList events;
    int currentEvent; //the next event to be handled, maybe.
    int pianoRollEvent;
    HighlightEveryKey keyHighlighter;
    #endregion

    #region midi-tolerance
    const float BEATS_TO_TOLERATE = 0.3f;
    //ArrayList tolerance;
    //HashSet<int> activeNotes;
    //represents key IDs that should be held down at the time with BEATS_TO_TOLERATE temporal tolerance.
    //int toleranceRunner;
    GameObject errorCube;
    #endregion

    // Use this for initialization
    void Start() {
        song = Song.GetCurrentSong();
        ErrorKeys = new Dictionary<int, float>();
        playhead = GameObject.Find("Playhead");
        /*       errorCube = GameObject.Find("ErrorIndicator");
               errorCube.SetActive(false);*/
        keyHighlighter = GameObject.FindObjectOfType<HighlightEveryKey>();

        ResetBars();

        #region gameObject_lockdown
        MusicSheet[0] = GameObject.Find("MusicSheet");
        MusicSheet[1] = GameObject.Find("MusicSheet_prev1");
        MusicSheet[2] = GameObject.Find("MusicSheet_next1");
        MusicSheet[3] = GameObject.Find("MusicSheet_next2");

        StaffLine1[0] = GameObject.Find("StaffLine1");
        StaffLine2[0] = GameObject.Find("StaffLine2");
        StaffLine3[0] = GameObject.Find("StaffLine3");
        StaffLine4[0] = GameObject.Find("StaffLine4");
        StaffLine5[0] = GameObject.Find("StaffLine5");
        StaffLine6[0] = GameObject.Find("StaffLine6");
        StaffLine7[0] = GameObject.Find("StaffLine7");
        StaffLine8[0] = GameObject.Find("StaffLine8");
        StaffLine9[0] = GameObject.Find("StaffLine9");
        StaffLine10[0] = GameObject.Find("StaffLine10");

        StaffLine1[1] = GameObject.Find("StaffLine1_prev1");
        StaffLine2[1] = GameObject.Find("StaffLine2_prev1");
        StaffLine3[1] = GameObject.Find("StaffLine3_prev1");
        StaffLine4[1] = GameObject.Find("StaffLine4_prev1");
        StaffLine5[1] = GameObject.Find("StaffLine5_prev1");
        StaffLine6[1] = GameObject.Find("StaffLine6_prev1");
        StaffLine7[1] = GameObject.Find("StaffLine7_prev1");
        StaffLine8[1] = GameObject.Find("StaffLine8_prev1");
        StaffLine9[1] = GameObject.Find("StaffLine9_prev1");
        StaffLine10[1] = GameObject.Find("StaffLine10_prev1");

        StaffLine1[2] = GameObject.Find("StaffLine1_next1");
        StaffLine2[2] = GameObject.Find("StaffLine2_next1");
        StaffLine3[2] = GameObject.Find("StaffLine3_next1");
        StaffLine4[2] = GameObject.Find("StaffLine4_next1");
        StaffLine5[2] = GameObject.Find("StaffLine5_next1");
        StaffLine6[2] = GameObject.Find("StaffLine6_next1");
        StaffLine7[2] = GameObject.Find("StaffLine7_next1");
        StaffLine8[2] = GameObject.Find("StaffLine8_next1");
        StaffLine9[2] = GameObject.Find("StaffLine9_next1");
        StaffLine10[2] = GameObject.Find("StaffLine10_next1");

        StaffLine1[3] = GameObject.Find("StaffLine1_next2");
        StaffLine2[3] = GameObject.Find("StaffLine2_next2");
        StaffLine3[3] = GameObject.Find("StaffLine3_next2");
        StaffLine4[3] = GameObject.Find("StaffLine4_next2");
        StaffLine5[3] = GameObject.Find("StaffLine5_next2");
        StaffLine6[3] = GameObject.Find("StaffLine6_next2");
        StaffLine7[3] = GameObject.Find("StaffLine7_next2");
        StaffLine8[3] = GameObject.Find("StaffLine8_next2");
        StaffLine9[3] = GameObject.Find("StaffLine9_next2");
        StaffLine10[3] = GameObject.Find("StaffLine10_next2");

        #endregion

        CurrentBar = 0;
    }

    bool HasSongChanged()
    {
        return Timing.songUpdated;
    }

    // Update is called once per frame
    void Update() {

        if (HasSongChanged())
        {
            Timing.ClearSongUpdatedFlag();
            // If the song has changed, #1 -- Get the new song and generate the pending roll key list. 
            song = Song.GetCurrentSong();
            events = SongAnalyzer.generateEventList(song);
            currentEvent = 0;
            pianoRollEvent = 0;

            // #2 - clear error keys, regardless of expiration. Not sure why, but without this step, 
            // keys can be redrawn as error keys after they are destroyed. 
            foreach (var key in ErrorKeys)
            {
                keyHighlighter.SetKeyHighlight(false, key.Key);
                ErrorKeys.Remove(key.Key);
            }

            // #3 - Destroy all piano roll keys, and prepare the first bar
            keyHighlighter.DestroyAllPianoRollItems();
            NextBar(0);
            LastBar = 1;
        }
        else
        {
            CurrentBar = Timing.GetCurrentBar();
            if (CurrentBar != LastBar)
            {
                NextBar(CurrentBar - 1);
            }
            LastBar = CurrentBar;
        }


        //figure out what events happened in the last frame and handle it!
        if (song != null)
        {
            /*
            //key highlighting.
            while (currentEvent < events.Count)
            {
                SongEvent e = (SongEvent)events[currentEvent];
                if (e.measureNumber <=Timing.CurrentMeasure)
                {
                    //process e.
                    keyHighlighter.SetKeyHighlight(e.isStart, e.keyID);
                    currentEvent++;
                }
                else { break; }
            }*/

            //tolerance updating:
            /*while (toleranceRunner < tolerance.Count)
            {
                SongEvent e = (SongEvent)tolerance[toleranceRunner];
                if (e.MeasureNumber <= Timing.CurrentMeasure)
                {
                    //process e.
                    if (e.IsStart) activeNotes.Add(e.KeyID);
                    else activeNotes.Remove(e.KeyID);
                    toleranceRunner++;
                }
                else { break; }
            }*/


            //piano roll
            float futureTick = Timing.CurrentMeasure + 2 * Timing.BeatsPerMeasure;
            while (pianoRollEvent < events.Count)
            {
                SongEvent e = (SongEvent)events[pianoRollEvent];
                if (e.EndPoint <= futureTick)
                {
                    //process e.
                    startDroplet(e, futureTick);
                    pianoRollEvent++;
                }
                else { break; }
            }

            // clear error keys, if they have expired
            foreach (var key in ErrorKeys)
            {
                if (key.Value < Timing.CurrentMeasure)
                {
                    keyHighlighter.SetKeyHighlight(false, key.Key);
                    ErrorKeys.Remove(key.Key);
                }
            }
        }

        
        Vector3 oldpos = playhead.transform.position;
        playhead.transform.position = new Vector3(GetPositionFromBeat(Timing.GetCurrentBeat(), 0), oldpos.y, oldpos.z);
    }

    private void startDroplet(SongEvent e, float currentMeasure)
    {
        if (e.IsStart) return;
        if (e.KeyID == 0) return;
        keyHighlighter.AddPianoRollItem(e, currentMeasure);
    }

    void NextBar(int currentBar)
    {
        // Check if song is null, and continue checking until it is not

        if (song == null)
        {
            song = Song.GetCurrentSong();
            events = SongAnalyzer.generateEventList(song);
            currentEvent = 0;
            pianoRollEvent = 0;
            /*
            tolerance = SongAnalyzer.generateEventList(song, BEATS_TO_TOLERATE);
            toleranceRunner = 0;
            activeNotes = new HashSet<int>();*/
        }
        else
        {
            ClearBar();

            if (currentBar == 0)
            {
                currentEvent = 0;
                pianoRollEvent = 0;
                //toleranceRunner = 0;
                bar_prev1 = new Bar(new List<Note>());
                foreach (var key in ErrorKeys)
                {
                    keyHighlighter.SetKeyHighlight(false, key.Key);
                    ErrorKeys.Remove(key.Key);
                }
            }
            else
            {
                bar_prev1 = song.GetBars()[currentBar - 1];
            }

            bar = song.GetBars()[currentBar];

            if (currentBar + 1 >= song.GetBars().Count)
            {
                bar_next1 = new Bar(new List<Note>());
            }
            else
            {
                bar_next1 = song.GetBars()[currentBar + 1];
            }

            if (currentBar + 2 >= song.GetBars().Count)
            {
                bar_next2 = new Bar(new List<Note>());
            }
            else
            {
                bar_next2 = song.GetBars()[currentBar + 2];
            }

            RenderBar(bar, 0, false); // 0 is current bar
            RenderBar(bar_prev1, 1, true); // 0 is current bar
            RenderBar(bar_next1, 2, true); // 0 is current bar
            RenderBar(bar_next2, 3, true); // 0 is current bar
        }
    }

    void ClearBar()
    {
        GameObject[] currentNotes = GameObject.FindGameObjectsWithTag("MicroNote");
        for (int i = 0; i < currentNotes.Length; i++)
        {
            Destroy(currentNotes[i]);
        }
    }

    void RenderBar(Bar bar, int whichBar, bool isSmall)
    {
        foreach (Note note in bar.GetNotes())
        {
            Instantiate((isSmall ? getTransformForNoteRatio_small(note) : getTransformForNoteRatio(note)),
                getNotePosition(note, whichBar), Quaternion.identity);
        }
    }

    Transform getTransformForNoteRatio_small(Note note)
    {
        switch (note.GetNoteType())
        {
            case Note.Note_Types.quarter_note:
                return quarter_note_small;
            case Note.Note_Types.half_note:
                return half_note_small;
            case Note.Note_Types.eighth_note:
                return eighth_note_small;
            case Note.Note_Types.whole_note:
                return whole_note_small;
            case Note.Note_Types.sixteenth_note:
                return sixteenth_note_small;
            case Note.Note_Types.dotted_quarter_note:
                return dotted_quarter_note_small;
            case Note.Note_Types.dotted_half_note:
                return dotted_half_note_small;
            case Note.Note_Types.dotted_eighth_note:
                return dotted_eighth_note_small;
            case Note.Note_Types.dotted_whole_note:
                return dotted_whole_note_small;
            case Note.Note_Types.dotted_sixteenth_note:
                return dotted_sixteenth_note_small;
            case Note.Note_Types.quarter_rest:
                return quarter_rest_small;
            case Note.Note_Types.half_rest:
                return half_rest_small;
            case Note.Note_Types.eighth_rest:
                return eighth_rest_small;
            case Note.Note_Types.whole_rest:
                return whole_rest_small;
            default:
                Debug.Log("Unknown note type (small)");
                return quarter_note;
        }
    }

    Transform getTransformForNoteRatio(Note note)
    {
        switch (note.GetNoteType())
        {
            case Note.Note_Types.quarter_note:
                return quarter_note;
            case Note.Note_Types.half_note:
                return half_note;
            case Note.Note_Types.eighth_note:
                return eighth_note;
            case Note.Note_Types.whole_note:
                return whole_note;
            case Note.Note_Types.sixteenth_note:
                return sixteenth_note;
            case Note.Note_Types.dotted_quarter_note:
                return dotted_quarter_note;
            case Note.Note_Types.dotted_half_note:
                return dotted_half_note;
            case Note.Note_Types.dotted_eighth_note:
                return dotted_eighth_note;
            case Note.Note_Types.dotted_whole_note:
                return dotted_whole_note;
            case Note.Note_Types.dotted_sixteenth_note:
                return dotted_sixteenth_note;
            case Note.Note_Types.quarter_rest:
                return quarter_rest;
            case Note.Note_Types.half_rest:
                return half_rest;
            case Note.Note_Types.eighth_rest:
                return eighth_rest;
            case Note.Note_Types.whole_rest:
                return whole_rest;
            default:
                Debug.Log("Unknown note type");
                return quarter_note;
        }
    }
    
    void ProcessKeyPress(int keyID)
    {
        bool isCorrect = false;
        foreach (SongEvent e in events)
        {
            if (e.KeyID == keyID && e.StartPoint < Timing.CurrentMeasure + TOLERANCE && e.StartPoint > Timing.CurrentMeasure - TOLERANCE)
            {
                //Debug.Log("You did it right. " + e.EndPoint + ", key: " + e.KeyID);
                isCorrect = true;
                e.SetHit();
                break;
            }
        }
        // if an incorrect press, show an error key
        if (!isCorrect)
        {
            ErrorKeys.Add(keyID, Timing.CurrentMeasure + ERROR_KEY_DURATION);
            keyHighlighter.SetKeyHighlight(true, keyID);
            Debug.Log("Nah gurr");
            // ShowErrorBlock(keyID);
        }
        /*
        if (activeNotes.Contains(keyID))
        {
            errorCube.SetActive(false);
            Debug.Log("yay!");
        }else
        {
            errorCube.SetActive(true);
            Debug.Log("nay!");
        }*/
    }

    Vector3 getNotePosition(Note note, int whichBar)
    {
        float x = 0;
        try
        {
            x = GetPositionFromBeat((float)note.GetStartingBeat(), whichBar);
        } catch (Exception e)
        {
            Debug.Log("note position fail");
        }

        float z = StaffLine5[whichBar].transform.position.z;

        float y = 0;
        float betweenLineDistance = (whichBar != 0) ? BETWEEN_STAFF_LINE_DISTANCE_small : BETWEEN_STAFF_LINE_DISTANCE;
        float lineOffset = (whichBar != 0) ? LINE_OFFSET_small : LINE_OFFSET;

        try
        {
            switch (note.GetPianoKey())
            {
                case Note.Rest_Treble:
                    y = StaffLine5[whichBar].transform.position.y;
                    break;
                case Note.Rest_Bass:
                    y = StaffLine10[whichBar].transform.position.y;
                    break;
                case Note.F_1:
                    y = StaffLine10[whichBar].transform.position.y - betweenLineDistance;
                    break;
                case Note.G_1:
                    y = StaffLine10[whichBar].transform.position.y;
                    break;
                case Note.A_1:
                    y = .5f * (StaffLine10[whichBar].transform.position.y + StaffLine9[whichBar].transform.position.y);
                    break;
                case Note.B_1:
                    y = StaffLine9[whichBar].transform.position.y;
                    break;
                case Note.C_2:
                    y = .5f * (StaffLine9[whichBar].transform.position.y + StaffLine8[whichBar].transform.position.y);
                    break;
                case Note.D_2:
                    y = StaffLine8[whichBar].transform.position.y;
                    break;
                case Note.E_2:
                    y = .5f * (StaffLine8[whichBar].transform.position.y + StaffLine7[whichBar].transform.position.y);
                    break;
                case Note.F_2:
                    y = StaffLine7[whichBar].transform.position.y;
                    break;
                case Note.G_2:
                    y = .5f * (StaffLine7[whichBar].transform.position.y + StaffLine6[whichBar].transform.position.y);
                    break;
                case Note.A_2:
                    y = StaffLine6[whichBar].transform.position.y;
                    break;
                case Note.B_2:
                    y = StaffLine6[whichBar].transform.position.y + betweenLineDistance;
                    break;
                case Note.C_3:
                    y = StaffLine6[whichBar].transform.position.y + betweenLineDistance + betweenLineDistance;
                    
                    // we need to add a ledger line, halfway between the two staves
                    Instantiate((whichBar != 0) ? ledger_line_small : ledger_line, new Vector3(x, .5f * (StaffLine6[whichBar].transform.position.y + StaffLine5[whichBar].transform.position.y), z), Quaternion.identity);
                    break;
                case Note.D_3:
                    y = StaffLine5[whichBar].transform.position.y - betweenLineDistance;
                    break;
                case Note.E_3:
                    y = StaffLine5[whichBar].transform.position.y;
                    break;
                case Note.F_3:
                    y = .5f * (StaffLine5[whichBar].transform.position.y + StaffLine4[whichBar].transform.position.y);
                    break;
                case Note.G_3:
                    y = StaffLine4[whichBar].transform.position.y;
                    break;
                case Note.A_3:
                    y = .5f * (StaffLine4[whichBar].transform.position.y + StaffLine3[whichBar].transform.position.y);
                    break;
                case Note.B_3:
                    y = StaffLine3[whichBar].transform.position.y;
                    break;
                case Note.C_4:
                    y = .5f * (StaffLine3[whichBar].transform.position.y + StaffLine2[whichBar].transform.position.y);
                    break;
                case Note.D_4:
                    y = StaffLine2[whichBar].transform.position.y;
                    break;
                case Note.E_4:
                    y = .5f * (StaffLine2[whichBar].transform.position.y + StaffLine1[whichBar].transform.position.y);
                    break;
                case Note.F_4:
                    y = StaffLine1[whichBar].transform.position.y;
                    break;
                case Note.G_4:
                    y = StaffLine1[whichBar].transform.position.y + betweenLineDistance;
                    break;
                default:
                    Debug.Log("Note " + note.GetPianoKey() + " is currently unsupported.");
                    y = StaffLine5[whichBar].transform.position.y;
                    break;
            }
            y -= lineOffset;
        } catch (Exception e)
        {
           // Debug.Log("Y");
        }

        

        //Debug.Log("X: " + x + ", Y: " + y + ", Z: " + z);

        return new Vector3(x, y, z);
    }

    float GetPositionFromBeat(float beat, int whichBar)
    {
        return (StaffLine5[whichBar].GetComponent<Renderer>().bounds.size.x * .95f) * (beat / (song.GetBeatsPerMeasure() + 1)) + MusicSheet[whichBar].transform.position.x - (.4f * StaffLine5[whichBar].GetComponent<Renderer>().bounds.size.x);
    }

    void ResetBars()
    {
        bar = new Bar(new List<Note>());
        bar_prev1 = new Bar(new List<Note>());
        bar_next1 = new Bar(new List<Note>());
        bar_next2 = new Bar(new List<Note>());
        currentEvent = 0;
        //toleranceRunner = 0;
    }

    public void ClearHighlights()
    {
        foreach (var key in ErrorKeys)
        {
            keyHighlighter.SetKeyHighlight(false, key.Key);
            ErrorKeys.Remove(key.Key);
        }
        keyHighlighter.DestroyAllPianoRollItems();
    }
}
