using System.Collections;
using NUnit.Framework;
using ReactUnity.ScriptEngine;
using ReactUnity.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace ReactUnity.Tests
{
    public class ScrollTests : TestBase
    {
        const string BaseScript = @"
            function App() {
                const globals = ReactUnity.useGlobals();
                return <>
                    <scroll direction={globals.direction}
                        alwaysShow={globals.alwaysShow}
                        sensitivity={globals.sensitivity}
                        elasticity={globals.elasticity}
                        smoothness={globals.smoothness}>
                        <view>
                        </view>
                    </scroll>
                </>;
            }

            Renderer.render(<GlobalsProvider children={<App />} />);
        ";

        const string BaseStyle = @"
            scroll {
                height: 200px;
                width: 200px;
            }
        ";

        public ScrollComponent Scroll => Q("scroll") as ScrollComponent;
        public UGUIComponent View => Q("view") as UGUIComponent;

        public ScrollTests(JavascriptEngineType engineType) : base(engineType) { }


        [ReactInjectableTest(BaseScript, BaseStyle)]
        public IEnumerator ScrollbarIsVisibleOnlyWhenSideOverflows()
        {
            yield return null;
            Assert.IsFalse(Scroll.ScrollRect.horizontalScrollbar.isActiveAndEnabled);
            Assert.IsFalse(Scroll.ScrollRect.verticalScrollbar.isActiveAndEnabled);

            View.Style.Set("width", 300);
            View.Style.Set("height", 300);
            yield return null;
            Assert.IsTrue(Scroll.ScrollRect.horizontalScrollbar.isActiveAndEnabled);
            Assert.IsTrue(Scroll.ScrollRect.verticalScrollbar.isActiveAndEnabled);


            View.Style.Set("width", 100);
            View.Style.Set("height", 300);
            yield return null;
            Assert.IsFalse(Scroll.ScrollRect.horizontalScrollbar.isActiveAndEnabled);
            Assert.IsTrue(Scroll.ScrollRect.verticalScrollbar.isActiveAndEnabled);

            View.Style.Set("width", 300);
            View.Style.Set("height", 100);
            yield return null;
            Assert.IsTrue(Scroll.ScrollRect.horizontalScrollbar.isActiveAndEnabled);
            Assert.IsFalse(Scroll.ScrollRect.verticalScrollbar.isActiveAndEnabled);

            View.Style.Set("width", 100);
        }

        private IEnumerator RunWithRandomRotation(System.Func<IEnumerator> cb)
        {
            var cube = GameObject.Find("Cube");

            for (int i = 0; i < 10; i++)
            {
                yield return cb();
                cube.transform.rotation = Random.rotation;
            }
        }

        [ReactInjectableTest(BaseScript, BaseStyle, customScene: ReactInjectableTestAttribute.WorldSceneName)]
        public IEnumerator ScrollbarCanBePositionedAndColoredWithStylingWorldScene()
        {
            yield return RunWithRandomRotation(ScrollbarCanBePositionedAndColoredWithStyling);
        }

        [ReactInjectableTest(BaseScript, BaseStyle, customScene: ReactInjectableTestAttribute.WorldSceneName)]
        public IEnumerator ScrollbarIsVisibleOnlyWhenSideOverflowsWorldScene()
        {
            yield return RunWithRandomRotation(ScrollbarIsVisibleOnlyWhenSideOverflows);
        }

        [ReactInjectableTest(BaseScript, BaseStyle)]
        public IEnumerator ScrollbarCanBePositionedAndColoredWithStyling()
        {
            View.Style.Set("width", 300);
            View.Style.Set("height", 300);

            Context.InsertStyle(@"
                scroll::scrollbar {
                    width: 20px;
                    height: 30px;
                }

                scroll::scrollbar[vertical] {
                    left: 5px;
                    top: 10px;
                    bottom: 15px;
                }

                scroll::scrollbar[horizontal] {
                    top: 5px;
                    left: 8px;
                    right: 16px;
                    background: red;
                }

                scroll::scrollbar[horizontal]::scrollbar-thumb {
                    background: blue;
                }
            ");

            yield return null;
            Assert.AreEqual(new Rect(-10, -87.5f, 20, 175), (Scroll.ScrollRect.verticalScrollbar.transform as RectTransform).rect);
            Assert.AreEqual(new Rect(-88, -15, 176, 30), (Scroll.ScrollRect.horizontalScrollbar.transform as RectTransform).rect);
            Assert.AreEqual(0, Scroll.ScrollRect.verticalScrollbar.transform.localPosition.z);
            Assert.AreEqual(0, Scroll.ScrollRect.horizontalScrollbar.transform.localPosition.z);

            var hbar = Q("scroll _scrollbar[horizontal]") as ScrollBarComponent;

            Assert.AreEqual(Scroll.ScrollRect.horizontalScrollbar, hbar.Scrollbar);
            Assert.AreEqual(Color.red, hbar.BorderAndBackground.Background.GetComponent<Image>().color);
            Assert.AreEqual(Color.blue, hbar.Thumb.BorderAndBackground.Background.GetComponent<Image>().color);
        }



        [ReactInjectableTest(BaseScript, BaseStyle)]
        public IEnumerator PropertiesGetAppliedToScrollbar()
        {
            View.Style.Set("width", 300);
            View.Style.Set("height", 300);

            yield return null;

            Assert.AreEqual(true, Scroll.ScrollRect.horizontalScrollbar.isActiveAndEnabled);
            Assert.AreEqual(true, Scroll.ScrollRect.verticalScrollbar.isActiveAndEnabled);
            Assert.AreEqual(true, Scroll.ScrollRect.horizontal);
            Assert.AreEqual(true, Scroll.ScrollRect.vertical);
            Assert.AreEqual(ScrollRect.ScrollbarVisibility.AutoHide, Scroll.ScrollRect.horizontalScrollbarVisibility);
            Assert.AreEqual(ScrollRect.ScrollbarVisibility.AutoHide, Scroll.ScrollRect.verticalScrollbarVisibility);
            Assert.AreEqual(50, Scroll.ScrollRect.scrollSensitivity);
            Assert.AreEqual(0.12f, Scroll.ScrollRect.SmoothScrollTime);
            Assert.AreEqual(ScrollRect.MovementType.Clamped, Scroll.ScrollRect.movementType);
            Assert.AreEqual(0, Scroll.ScrollRect.elasticity);


            Globals.Set("sensitivity", 100);
            Globals.Set("direction", "vertical");
            Globals.Set("alwaysShow", "both");
            Globals.Set("elasticity", 0.5f);
            Globals.Set("smoothness", 0.4f);
            yield return null;

            Assert.AreEqual(false, Scroll.ScrollRect.horizontalScrollbar.isActiveAndEnabled);
            Assert.AreEqual(true, Scroll.ScrollRect.verticalScrollbar.isActiveAndEnabled);
            Assert.AreEqual(false, Scroll.ScrollRect.horizontal);
            Assert.AreEqual(true, Scroll.ScrollRect.vertical);
            Assert.AreEqual(ScrollRect.ScrollbarVisibility.Permanent, Scroll.ScrollRect.horizontalScrollbarVisibility);
            Assert.AreEqual(ScrollRect.ScrollbarVisibility.Permanent, Scroll.ScrollRect.verticalScrollbarVisibility);
            Assert.AreEqual(100, Scroll.ScrollRect.scrollSensitivity);
            Assert.AreEqual(0.4f, Scroll.ScrollRect.SmoothScrollTime);
            Assert.AreEqual(ScrollRect.MovementType.Elastic, Scroll.ScrollRect.movementType);
            Assert.AreEqual(0.5f, Scroll.ScrollRect.elasticity);


            Globals.Set("elasticity", 0);
            Globals.Set("smoothness", 0);
            yield return null;
            Assert.AreEqual(0, Scroll.ScrollRect.SmoothScrollTime);
            Assert.AreEqual(ScrollRect.MovementType.Clamped, Scroll.ScrollRect.movementType);
            Assert.AreEqual(0, Scroll.ScrollRect.elasticity);
        }


        [ReactInjectableTest(BaseScript, BaseStyle, realTimer: true)]
        public IEnumerator ScrollCanBeDoneByCode()
        {
            View.Style.Set("width", 400);
            View.Style.Set("height", 420);
            yield return null;

            Assert.AreEqual(0, Scroll.ScrollTop, 1);
            Assert.AreEqual(0, Scroll.ScrollLeft, 1);
            Assert.AreEqual(200, Scroll.ClientWidth, 1);
            Assert.AreEqual(200, Scroll.ClientHeight, 1);
            Assert.AreEqual(400, Scroll.ScrollWidth, 1);
            Assert.AreEqual(420, Scroll.ScrollHeight, 1);
            Assert.AreEqual(0, Scroll.ScrollRect.content.offsetMin.x, 1);
            Assert.AreEqual(0, Scroll.ScrollRect.content.offsetMax.y, 1);

            Scroll.ScrollTop = 100;
            Scroll.ScrollLeft = 70;

            Assert.AreEqual(100, Scroll.ScrollTop, 1);
            Assert.AreEqual(70, Scroll.ScrollLeft, 1);
            Assert.AreEqual(200, Scroll.ClientWidth, 1);
            Assert.AreEqual(200, Scroll.ClientHeight, 1);
            Assert.AreEqual(-70, Scroll.ScrollRect.content.offsetMin.x, 1);
            Assert.AreEqual(100, Scroll.ScrollRect.content.offsetMax.y, 1);


            Scroll.ScrollTop = 320;
            Scroll.ScrollLeft = 360;
            Assert.AreEqual(200, Scroll.ScrollLeft, 1);
            Assert.AreEqual(220, Scroll.ScrollTop, 1);


            Scroll.ScrollTo(120, 140, 0);
            Assert.AreEqual(120, Scroll.ScrollLeft, 1);
            Assert.AreEqual(140, Scroll.ScrollTop, 1);


            Scroll.ScrollBy(10, 30, 0);
            Assert.AreEqual(130, Scroll.ScrollLeft, 1);
            Assert.AreEqual(170, Scroll.ScrollTop, 1);
            Assert.AreEqual(-130, Scroll.ScrollRect.content.offsetMin.x, 1);
            Assert.AreEqual(170, Scroll.ScrollRect.content.offsetMax.y, 1);


            Scroll.SetProperty("smoothness", 0.4f);
            Scroll.ScrollBy(-100, -150);
            yield return AdvanceTime(0.45f);
            Assert.AreEqual(30, Scroll.ScrollLeft, 1);
            Assert.AreEqual(20, Scroll.ScrollTop, 1);


            View.Style.Set("width", 400);
            View.Style.Set("height", 420);
            yield return null;

            Assert.AreEqual(30, Scroll.ScrollLeft, 1);
            Assert.AreEqual(20, Scroll.ScrollTop, 1);
            Assert.AreEqual(200, Scroll.ClientWidth, 1);
            Assert.AreEqual(200, Scroll.ClientHeight, 1);
            Assert.AreEqual(200, Scroll.ScrollWidth, 1);
            Assert.AreEqual(200, Scroll.ScrollHeight, 1);

        }
    }
}
