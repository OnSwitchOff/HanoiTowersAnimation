using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskCompletionSource<bool> completionSource;

        private Stack<int>[] stacks = new[] { new Stack<int>(), new Stack<int>(), new Stack<int>() };

        public MainWindow()
        {
            InitializeComponent();
            stacks[0].Push(4);
            stacks[0].Push(3);
            stacks[0].Push(2);
            stacks[0].Push(1); 
        }

        private async Task hanoy(int blocksCount, int currentPin, int destinationPin)
        {
            if (blocksCount == 1)
            {
                Debug.WriteLine($"move {blocksCount} from {currentPin} to {destinationPin}\n");
                await MoveBlock(blocksCount, currentPin, destinationPin);

            } 
            else
            {
                int tmpPin = 3 - currentPin - destinationPin;
                await hanoy(blocksCount - 1, currentPin, tmpPin);
                Debug.WriteLine($"move {blocksCount} from {currentPin} to {destinationPin}\n");
                await MoveBlock(blocksCount, currentPin, destinationPin);
                await hanoy(blocksCount - 1, tmpPin, destinationPin);
            }
        }

        UIElement curPin;
        UIElement destPin;
        UIElement curBlock;
        int destPinIndex;
        private Task MoveBlock(int blockNumber, int currentPin, int destinationPin)
        {
            completionSource = new TaskCompletionSource<bool>();

            destPinIndex = destinationPin;
            stacks[destinationPin].Push(stacks[currentPin].Pop());

            string elementName = "pin" + (currentPin + 1);
            curPin = FindName(elementName) as UIElement;          

            elementName = "pin" + (destinationPin + 1);
            destPin = FindName(elementName) as UIElement;

            elementName = "block" + blockNumber;
            curBlock = FindName(elementName) as UIElement;

            DoubleAnimation handAnimation = new DoubleAnimation();
            handAnimation.From = Canvas.GetLeft(hand);
            handAnimation.To = Canvas.GetLeft(curPin) - 15; 
            handAnimation.Duration = TimeSpan.FromSeconds(0.5);
            handAnimation.RepeatBehavior = new RepeatBehavior(1f);
            handAnimation.Completed += SlideHandAnimationComplete;
            hand.BeginAnimation(Canvas.LeftProperty, handAnimation);         

            return completionSource.Task;
        }

        private void SlideHandAnimationComplete(object sender, EventArgs e)
        {
            DoubleAnimation handAnimation = new DoubleAnimation();
            handAnimation.From = Canvas.GetTop(hand);
            handAnimation.To = Canvas.GetTop(curBlock) - hand.Height;
            handAnimation.Duration = TimeSpan.FromSeconds(1);
            handAnimation.RepeatBehavior = new RepeatBehavior(1f);
            handAnimation.Completed += HandAnimation_Completed;
            hand.BeginAnimation(Canvas.TopProperty, handAnimation);
        }

        private void HandAnimation_Completed(object sender, EventArgs e)
        {
            DoubleAnimation handAnimation = new DoubleAnimation();
            handAnimation.From = Canvas.GetTop(curBlock) - hand.Height; 
            handAnimation.To = -20;
            handAnimation.Duration = TimeSpan.FromSeconds(1);
            handAnimation.RepeatBehavior = new RepeatBehavior(1f); 

            DoubleAnimation blockAnimation = new DoubleAnimation();
            blockAnimation.From = Canvas.GetTop(curBlock);
            blockAnimation.To = 0;
            blockAnimation.Duration = TimeSpan.FromSeconds(1);
            blockAnimation.AutoReverse = false;
            blockAnimation.RepeatBehavior = new RepeatBehavior(1f);
            blockAnimation.Completed += LiftAnimationComplete;

            hand.BeginAnimation(Canvas.TopProperty, handAnimation);
            curBlock.BeginAnimation(Canvas.TopProperty, blockAnimation);
        }

        private void LiftAnimationComplete(object sender, EventArgs e)
        {
            DoubleAnimation handAnimation = new DoubleAnimation();
            handAnimation.From = Canvas.GetLeft(hand);
            handAnimation.To = Canvas.GetLeft(hand) + Canvas.GetLeft(destPin) - Canvas.GetLeft(curPin);
            handAnimation.Duration = TimeSpan.FromSeconds(0.5);
            handAnimation.RepeatBehavior = new RepeatBehavior(1f);

            DoubleAnimation blockAnimation = new DoubleAnimation();
            blockAnimation.From = Canvas.GetLeft(curBlock);
            blockAnimation.To = Canvas.GetLeft(curBlock) + Canvas.GetLeft(destPin) - Canvas.GetLeft(curPin);
            blockAnimation.Duration = TimeSpan.FromSeconds(0.5);
            blockAnimation.AutoReverse = false;
            blockAnimation.RepeatBehavior = new RepeatBehavior(1f);
            blockAnimation.Completed += SlideAnimationComplete;

            hand.BeginAnimation(Canvas.LeftProperty, handAnimation);
            curBlock.BeginAnimation(Canvas.LeftProperty, blockAnimation);
        }

        private void SlideAnimationComplete(object sender, EventArgs e)
        {
            DoubleAnimation handAnimation = new DoubleAnimation();
            handAnimation.From = Canvas.GetTop(hand);
            handAnimation.To = 500 - 20 * stacks[destPinIndex].Count - hand.Height;
            handAnimation.Duration = TimeSpan.FromSeconds(1);
            handAnimation.RepeatBehavior = new RepeatBehavior(1f);

            DoubleAnimation blockAnimation = new DoubleAnimation();
            blockAnimation.From = Canvas.GetTop(curBlock);
            blockAnimation.To = 500 - 20 * stacks[destPinIndex].Count;
            blockAnimation.Duration = TimeSpan.FromSeconds(1);
            blockAnimation.AutoReverse = false;
            blockAnimation.RepeatBehavior = new RepeatBehavior(1f);
            blockAnimation.Completed += PutAnimationComplete;

            hand.BeginAnimation(Canvas.TopProperty, handAnimation);
            curBlock.BeginAnimation(Canvas.TopProperty, blockAnimation);
        }

        private void PutAnimationComplete(object sender, EventArgs e)
        {
            DoubleAnimation handAnimation = new DoubleAnimation();
            handAnimation.From = Canvas.GetTop(hand);
            handAnimation.To = - 20;
            handAnimation.Duration = TimeSpan.FromSeconds(1);
            handAnimation.RepeatBehavior = new RepeatBehavior(1f);
            handAnimation.Completed += HandLastLiftComplete;
            hand.BeginAnimation(Canvas.TopProperty, handAnimation);
        }

        private void HandLastLiftComplete(object sender, EventArgs e)
        {
            completionSource.SetResult(true);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (completionSource != null)
            {
                return;
            }

            for (int i = 0; i < stacks.Length; i++)
            {
                if (stacks[i].Count == 4)
                {
                    startBtn.Visibility = Visibility.Collapsed;
                    await hanoy(4, i, (i + 2) % stacks.Length);
                    completionSource = null;
                    startBtn.Visibility = Visibility.Visible;
                    return;
                }
            }
        }
    }
}
