using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;
//using ReactiveUI.Fody.Helpers;

namespace EVTUI.ViewModels;

public class FieldBase : ReactiveObject
{
    public FieldBase(string name, bool editable)
    {
        this.Name     = name;
        this.Editable = editable;
    }

    public string Name     { get; }
    public bool   Editable { get; }
}

// ints/floats with open ranges
// (to be numerical entry)
public class NumEntryField : FieldBase
{
    public NumEntryField(string name, bool editable, dynamic val, dynamic? lowerLimit, dynamic? upperLimit, dynamic increment) : base(name, editable)
    {
        _value          = (decimal)val;
        this.LowerLimit = (decimal?)lowerLimit;
        this.UpperLimit = (decimal?)upperLimit;
        this.Increment  = (decimal)increment;
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public decimal? LowerLimit { get; set; }
    public decimal? UpperLimit { get; set; }
    public decimal  Increment  { get; set; }
}

// TODO: enable inline non-string elements...?
public class StringEntryField : FieldBase
{
    public StringEntryField(string name, bool editable, string text) : base(name, editable)
    {
        _text = text;
    }

    private string _text;
    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
}

// ints/floats with definite ranges
// (to be sliders)
public class NumRangeField : FieldBase
{
    public NumRangeField(string name, bool editable, dynamic val, dynamic lowerLimit, dynamic upperLimit, dynamic? increment) : base(name, editable)
    {
        _value          = (decimal)val;
        this.LowerLimit = (decimal)lowerLimit;
        this.UpperLimit = (decimal)upperLimit;
        this.Increment  = (decimal?)increment;
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public decimal  LowerLimit { get; set; }
    public decimal  UpperLimit { get; set; }
    public decimal? Increment  { get; set; }
}

// indices, maybe unknown enums
// (to be dropdowns)
public class IntSelectionField : FieldBase
{
    public IntSelectionField(string name, bool editable, int choiceIndex, List<int> choices) : base(name, editable)
    {
        _choice = choiceIndex;
        _choices = new ObservableCollection<int>(choices);
    }

    private int _choice;
    public int Choice
    {
        get => _choice;
        set => this.RaiseAndSetIfChanged(ref _choice, value);
    }
    //private readonly ObservableAsPropertyHelper<int> _choice;
    //public int Choice => _choice.Value;

    private ObservableCollection<int> _choices;
    public ObservableCollection<int> Choices
    {
        get => _choices;
        set => this.RaiseAndSetIfChanged(ref _choices, value);
    }
}

// known enums
// (to be dropdowns)
public class StringSelectionField : FieldBase
{
    public StringSelectionField(string name, bool editable, string choiceIndex, List<string> choices) : base(name, editable)
    {
        _choice = choiceIndex;
        _choices = new ObservableCollection<string>(choices);
    }

    private string _choice;
    public string Choice
    {
        get => _choice;
        set => this.RaiseAndSetIfChanged(ref _choice, value);
    }

    private ObservableCollection<string> _choices;
    public ObservableCollection<string> Choices
    {
        get => _choices;
        set => this.RaiseAndSetIfChanged(ref _choices, value);
    }
}

// (to be checkboxes)
public class BoolChoiceField : FieldBase
{
    public BoolChoiceField(string name, bool editable, bool val) : base(name, editable)
    {
        _value = val;
    }

    private bool _value;
    public bool Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}

public class Timeline : ReactiveObject
{

    public Timeline(DataManager dataManager)
    {
        this.Frames = new List<Frame>();
		for (int i=0; i<dataManager.EventManager.EventDuration; i++)
            this.Frames.Add(new Frame(i));
        this.FrameCount = dataManager.EventManager.EventDuration;
        this.ActiveFrame = 0;

        this.Categories = new List<Category>();
        this.Categories.Add(new Category("Field",    1,  this.FrameCount));
        this.Categories.Add(new Category("Env",      2,  this.FrameCount));
        this.Categories.Add(new Category("Camera",   3,  this.FrameCount));
        this.Categories.Add(new Category("Model",    4,  this.FrameCount));
        this.Categories.Add(new Category("Effect",   5,  this.FrameCount));
        this.Categories.Add(new Category("Crowd",    6,  this.FrameCount));
        this.Categories.Add(new Category("Image",    7,  this.FrameCount));
        this.Categories.Add(new Category("Movie",    8,  this.FrameCount));
        this.Categories.Add(new Category("Dialogue", 9,  this.FrameCount));
        this.Categories.Add(new Category("Texture",  10, this.FrameCount));
        this.Categories.Add(new Category("UI",       11, this.FrameCount));
        this.Categories.Add(new Category("Post",     12, this.FrameCount));
        this.Categories.Add(new Category("Audio",    13, this.FrameCount));
        this.Categories.Add(new Category("Script",   14, this.FrameCount));
        this.Categories.Add(new Category("Timing",   15, this.FrameCount));
        this.Categories.Add(new Category("Hardware", 16, this.FrameCount));
        this.Categories.Add(new Category("Other",    17, this.FrameCount));

        for (int j=0; j<dataManager.EventManager.EventSoundCommands.Length; j++)
        {
            string code = dataManager.EventManager.EventSoundCommands[j].CommandCode;
            int i = dataManager.EventManager.EventSoundCommands[j].FrameStart;
            int len = dataManager.EventManager.EventSoundCommands[j].FrameDuration;
            if (i >= 0 && i < this.Frames.Count)
            {
                CommandPointer newCmd = new CommandPointer(code, true, j, i, len);
                this.Categories[12].AddCommand(newCmd);
            }
        }
        for (int j=0; j<dataManager.EventManager.EventCommands.Length; j++)
        {
            string code = dataManager.EventManager.EventCommands[j].CommandCode;
            int i = dataManager.EventManager.EventCommands[j].FrameStart;
            int len = dataManager.EventManager.EventCommands[j].FrameDuration;
            if (i >= 0 && i < this.Frames.Count)
            {
                CommandPointer newCmd = new CommandPointer(code, false, j, i, len);
                int catInd = -1;
                if (code == "FbEn" || code == "Flbk" || code.StartsWith("Im"))
                    catInd = 6;
                else if (code == "Msg_" || code == "MsgR" || code == "Cht_")
                    catInd = 8;
                else if (code == "LBX_" || code == "Date")
                    catInd = 10;
                else if (code == "AlEf" || code.StartsWith("G"))
                    catInd = 11;
                else if (code == "Scr_")
                    catInd = 13;
                else if (code == "Chap" || code == "FrJ_")
                    catInd = 14;
                else if (code == "PRum" || code == "TrMc")
                    catInd = 15;
                else if (code.StartsWith("En"))
                    catInd = 1;
                else if (code.StartsWith("Cw"))
                    catInd = 5;
                else if (code.StartsWith("Mv"))
                    catInd = 7;
                else if (code.StartsWith("Fd"))
                    catInd = 10;
                else if (code.StartsWith("F"))
                    catInd = 0;
                else if (code.StartsWith("C"))
                    catInd = 2;
                else if (code.StartsWith("M"))
                    catInd = 3;
                else if (code.StartsWith("E"))
                    catInd = 4;
                else if (code.StartsWith("T"))
                    catInd = 9;
                else if (code.StartsWith("P"))
                    catInd = 11;
                else
                    catInd = 16;
                if (catInd > -1)
                    this.Categories[catInd].AddCommand(newCmd);
            }
        }
    }

    public int FrameCount { get; set; }

    public List<Frame>          Frames     { get; set; }
    public List<Category>       Categories { get; set; }

    private int _activeFrame;
    public int ActiveFrame
    {
        get => _activeFrame;
        set => this.RaiseAndSetIfChanged(ref _activeFrame, value);
    }
}

public class Frame
{
    public Frame(int index)
    {
        this.Index    = index;
        this.Name     = (index+1).ToString();
    }

    public int                  Index    { get; set; }
    public string               Name     { get; set; }
}

public class Category : ViewModelBase
{
    public Category(string name, int index, int frameCount)
    {
        this.FrameCount = frameCount;
        this.Name  = name;
        this.Index = index;
        this.Commands = new List<CommandPointer>();
        this.MaxInOneFrame = 0;
        this.FrameCounts = new Dictionary<int, int>();
        this.IsOpen = true;
    }

    private Dictionary<int, int> FrameCounts;

    public void AddCommand(CommandPointer newCmd)
    {
        if (!(this.FrameCounts.ContainsKey(newCmd.Frame)))
            this.FrameCounts[newCmd.Frame] = 0;
        newCmd.PositionWithinFrame = this.FrameCounts[newCmd.Frame];
        this.FrameCounts[newCmd.Frame] += 1;
        if (this.FrameCounts[newCmd.Frame] > this.MaxInOneFrame)
            this.MaxInOneFrame = this.FrameCounts[newCmd.Frame];
        this.Commands.Add(newCmd);
    }

    public int FrameCount    { get; set; }
    public int MaxInOneFrame { get; set; }

    public string Name  { get; set; }
    public int    Index { get; set; }

    public List<CommandPointer> Commands   { get; set; }
    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isOpen, value);
            OnPropertyChanged(nameof(IsOpen));
        }
    }
}

public class CommandPointer
{
    public CommandPointer(string code, bool isAudioCmd, int cmdIndex, int frame, int duration)
    {
        this.Code       = code;
        this.IsAudioCmd = isAudioCmd;
        this.CmdIndex   = cmdIndex;
        this.Frame      = frame;
        this.Duration   = duration;
        this.PositionWithinFrame = 0;
    }

    public string Code       { get; }
    public bool   IsAudioCmd { get; }
    public int    CmdIndex   { get; }
    public int    Frame      { get; }
    public int    Duration   { get; }

    public int    PositionWithinFrame { get; set; }
}

public class TimelinePanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }

    public Timeline TimelineContent { get; set; }
    public dynamic ActiveCommand { get; set; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public TimelinePanelViewModel(DataManager dataManager)
    {
        this.Config = dataManager;
        this.TimelineContent = new Timeline(dataManager);
    }

    public void SetActiveCommand(CommandPointer cmd)
    {
        SerialCommand command     = ((cmd.IsAudioCmd) ? this.Config.EventManager.EventSoundCommands : this.Config.EventManager.EventCommands)[cmd.CmdIndex];
        dynamic       commandData = ((cmd.IsAudioCmd) ? this.Config.EventManager.EventSoundCommandData : this.Config.EventManager.EventCommandData)[cmd.CmdIndex];
        Type          commandType = Type.GetType($"EVTUI.ViewModels.TimelineCommands.{cmd.Code}");
		if (commandType is null)
            this.ActiveCommand = new TimelineCommands.Generic(this.Config, command, commandData);
        else
            this.ActiveCommand = Activator.CreateInstance(commandType, new object[] { this.Config, command, commandData });
    }

    public void UnsetActiveCommand(bool saveFirst)
    {
        if (!this.Config.ReadOnly && saveFirst)
            this.ActiveCommand.SaveChanges();
        this.ActiveCommand = null;
    }

    public void PlayCueFromSource(string source, int cueId, int trackIndex)
    {
        if (this.Config.AudioManager.AcbByType[source].Count > 0)
        {
            this.Config.AudioManager.ActiveACB = this.Config.AudioManager.AcbByType[source][0];
            this.Config.AudioManager.PlayCueTrack((uint)cueId, trackIndex, this.Config.ProjectManager.AdxKey);
        }
    }

}
