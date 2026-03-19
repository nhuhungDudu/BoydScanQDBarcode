using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BoydScanQDBarcode.Helpers;
public static class RichTextBoxHelper
{
    public static readonly DependencyProperty LogItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "LogItemsSource",
            typeof(ObservableCollection<string>),
            typeof(RichTextBoxHelper),
            new PropertyMetadata(null, OnLogItemsSourceChanged));

    public static void SetLogItemsSource(DependencyObject element, ObservableCollection<string> value)
    {
        element.SetValue(LogItemsSourceProperty, value);
    }

    public static ObservableCollection<string> GetLogItemsSource(DependencyObject element)
    {
        return (ObservableCollection<string>)element.GetValue(LogItemsSourceProperty);
    }

    private static void OnLogItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RichTextBox rtb)
        {
            // Hủy đăng ký sự kiện cũ (nếu có) để tránh rò rỉ bộ nhớ
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= (sender, args) => OnCollectionChanged(rtb, args);
            }

            // Đăng ký sự kiện mới
            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                rtb.Document.Blocks.Clear(); // Xóa nội dung nếu gán một list hoàn toàn mới

                // FIX LỖI MẤT LOG KHI CHUYỂN TRANG: 
                // Kiểm tra xem danh sách đã có dữ liệu chưa, nếu có thì vẽ lại ngay lập tức
                if (e.NewValue is IEnumerable<string> existingItems && existingItems.Any())
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var batch in existingItems)
                        {
                            AddBatchToRichTextBox(rtb, batch);
                        }
                        rtb.ScrollToEnd();
                    });
                }

                newCollection.CollectionChanged += (sender, args) => OnCollectionChanged(rtb, args);
            }
        }
    }

    private static void OnCollectionChanged(RichTextBox rtb, NotifyCollectionChangedEventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (string batch in e.NewItems)
                {
                    AddBatchToRichTextBox(rtb, batch);
                }

                rtb.ScrollToEnd();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (rtb.Document.Blocks.FirstBlock != null)
                    {
                        rtb.Document.Blocks.Remove(rtb.Document.Blocks.FirstBlock);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                rtb.Document.Blocks.Clear();
            }
        });
    }

    // Tách phần logic xử lý text, tô màu và ngắt dòng ra hàm riêng để tái sử dụng
    private static void AddBatchToRichTextBox(RichTextBox rtb, string batch)
    {
        // 1. FIX LỖI CÁCH DÒNG: Bắt buộc phải có Margin = 0 để xóa lề mặc định của Paragraph
        var paragraph = new Paragraph { Margin = new Thickness(0) };

        string[] lines = batch.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var validLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        for (int i = 0; i < validLines.Count; i++)
        {
            string line = validLines[i];
            var run = new Run(line);

            string upperLine = line.ToUpper();

            // PHÂN LOẠI VÀ TÔ MÀU
            if (upperLine.Contains("ERROR") || upperLine.Contains("FAIL"))
            {
                run.Foreground = Brushes.Red;
            }
            else if (upperLine.Contains("WARN"))
            {
                run.Foreground = Brushes.DarkOrange;
            }
            else if (upperLine.Contains("PASS") || upperLine.Contains("SUCCESS"))
            {
                run.Foreground = Brushes.ForestGreen;
            }
            else
            {
                // Log Info bình thường
                run.Foreground = Brushes.Black; // Hoặc dùng Brushes.DarkSlateGray cho dịu mắt
            }

            paragraph.Inlines.Add(run);

            // 2. FIX LỖI NÍU DÒNG: Chèn thêm Enter giữa các log trong cùng 1 mẻ
            if (i < validLines.Count - 1)
            {
                paragraph.Inlines.Add(new LineBreak());
            }
        }

        rtb.Document.Blocks.Add(paragraph);
    }
}
