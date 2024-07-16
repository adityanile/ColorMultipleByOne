using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Tools.Image.Base
{
    public abstract class BasePaintToolSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool canPaintLines = true;
        [PaintToolSettings] public bool CanPaintLines
        {
            get => canPaintLines;
            set
            {
                canPaintLines = value; 
                OnPropertyChanged();
            }
        }

        private bool drawOnBrushMove;
        [PaintToolSettings] public bool DrawOnBrushMove
        {
            get => drawOnBrushMove;
            set
            {
                drawOnBrushMove = value;
                OnPropertyChanged();
            }
        }
        
        protected IPaintData Data;

        protected BasePaintToolSettings(IPaintData paintData)
        {
            Data = paintData;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) 
                return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}