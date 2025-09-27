using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MusicTheory.Theory.Time;

namespace NoteValueZoom.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly List<(long start, Note note)> _items = new();
        public MainWindow()
        {
            InitializeComponent();
            // 初期化: 4つのノートを並べる
            long t=0; var ds = new[]{ DurationFactory.Eighth(), DurationFactory.Quarter(), DurationFactory.Sixteenth(), DurationFactory.Quarter(1)};
            foreach (var d in ds){ _items.Add((t, new Note(d))); t += d.Ticks; }
            RefreshList();
        }

        private void RefreshList()
        {
            List.ItemsSource = null;
            List.ItemsSource = _items.Select(x => new ViewItem(x.start, x.note)).ToList();
        }

        private void ApplyZoomSelected(int delta)
        {
            var selected = List.SelectedItems.Cast<ViewItem>().ToList();
            if (selected.Count == 0) return;
            var map = selected.ToDictionary(v => v.Start);
            for (int i=0;i<_items.Count;i++)
            {
                var (s, n) = _items[i];
                if (map.ContainsKey(s))
                    _items[i] = (s, new Note(NoteValueZoom.Zoom(n.Duration, delta), n.Pitch, n.Velocity, n.Channel));
            }
            RefreshList();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e) => ApplyZoomSelected(-1);
        private void ZoomIn_Click(object sender, RoutedEventArgs e) => ApplyZoomSelected(+1);

        private record ViewItem(long Start, Note Note)
        {
            public string Label => $"{Start,5} : {Note.Duration.Ticks} ticks";
            public override string ToString() => Label;
        }
    }
}
