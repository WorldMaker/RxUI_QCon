using System;
using Xunit;

namespace RxUI_QCon.Tests
{
    public class MainWindowViewModelTests
    {
        [Fact]
        public void ColorShouldChangeWhenValuesChange()
        {
            dynamic fixture = new MainWindowController();

            fixture.Red = 255;

            Assert.NotNull(fixture.FinalColor);
            Assert.Equal(255, fixture.FinalColor.Color.R);
            Assert.Equal(0, fixture.FinalColor.Color.B);

            fixture.Blue = 300;

            Assert.NotNull(fixture.FinalColor);
            Assert.Equal(255, fixture.FinalColor.Color.R);
            Assert.Equal(0, fixture.FinalColor.Color.B);

            fixture.Blue = 128;

            Assert.NotNull(fixture.FinalColor);
            Assert.Equal(255, fixture.FinalColor.Color.R);
            Assert.Equal(128, fixture.FinalColor.Color.B);
        }

        [Fact]
        public void CantHitOkWhenValuesAreBogus()
        {
            dynamic New = ImpromptuInterface.Dynamic.Builder.New<MainWindowController>();
            dynamic fixture = New.Test(Red: 0, Green: 0, Blue: 0);

            Assert.True(fixture.Command.Ok.CanExecute(null));

            fixture.Red = -4;
            Assert.False(fixture.Command.Ok.CanExecute(null));

            fixture.Red = 128;
            Assert.True(fixture.Command.Ok.CanExecute(null));
        }
    }
}
