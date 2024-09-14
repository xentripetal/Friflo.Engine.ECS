// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Friflo.Editor.UI.Inspector;

class InspectorControlModel : INotifyPropertyChanged
{
    private int componentCount;
    private int entityId;
    private int scriptCount;
    private int tagCount;

    internal int EntityId
    {
        get => entityId;
        set
        {
            entityId = value;
            OnPropertyChanged();
        }
    }
    internal int TagCount
    {
        get => tagCount;
        set
        {
            tagCount = value;
            OnPropertyChanged();
        }
    }
    internal int ComponentCount
    {
        get => componentCount;
        set
        {
            componentCount = value;
            OnPropertyChanged();
        }
    }
    internal int ScriptCount
    {
        get => scriptCount;
        set
        {
            scriptCount = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
