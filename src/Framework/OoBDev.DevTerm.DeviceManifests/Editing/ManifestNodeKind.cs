using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>What one entry in the manifest editor's outline is.</summary>
public enum ManifestNodeKind
{
    Identity,
    Commands,
    Command,
    Parameter,
    Patterns,
    Pattern,
    Frame,
    FrameField,
    Panel,
    Section,
    Control,
}
