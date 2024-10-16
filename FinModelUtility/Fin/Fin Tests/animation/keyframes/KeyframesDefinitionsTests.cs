﻿using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.animation.keyframes;

public class KeyframeDefinitionsTests {
  [Test]
  public void TestAddToEnd() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(0, "0");
    impl.SetKeyframe(1, "1");
    impl.SetKeyframe(2, "2");
    impl.SetKeyframe(3, "3");
    impl.SetKeyframe(4, "4");

    AssertKeyframes_(impl,
                     new KeyframeDefinition<string>(0, "0"),
                     new KeyframeDefinition<string>(1, "1"),
                     new KeyframeDefinition<string>(2, "2"),
                     new KeyframeDefinition<string>(3, "3"),
                     new KeyframeDefinition<string>(4, "4")
    );
  }

  [Test]
  public void TestReplace() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(1, "first");
    impl.SetKeyframe(1, "second");
    impl.SetKeyframe(1, "third");

    AssertKeyframes_(impl, new KeyframeDefinition<string>(1, "third"));
  }

  [Test]
  public void TestInsertAtFront() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(4, "4");
    impl.SetKeyframe(5, "5");
    impl.SetKeyframe(2, "2");
    impl.SetKeyframe(1, "1");
    impl.SetKeyframe(0, "0");

    AssertKeyframes_(impl,
                     new KeyframeDefinition<string>(0, "0"),
                     new KeyframeDefinition<string>(1, "1"),
                     new KeyframeDefinition<string>(2, "2"),
                     new KeyframeDefinition<string>(4, "4"),
                     new KeyframeDefinition<string>(5, "5")
    );
  }

  [Test]
  public void TestInsertInMiddle() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(0, "0");
    impl.SetKeyframe(9, "9");
    impl.SetKeyframe(5, "5");
    impl.SetKeyframe(2, "2");
    impl.SetKeyframe(7, "7");

    AssertKeyframes_(impl,
                     new KeyframeDefinition<string>(0, "0"),
                     new KeyframeDefinition<string>(2, "2"),
                     new KeyframeDefinition<string>(5, "5"),
                     new KeyframeDefinition<string>(7, "7"),
                     new KeyframeDefinition<string>(9, "9")
    );
  }

  [Test]
  public void TestHugeRange() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(1000, "1000");
    impl.SetKeyframe(2, "2");
    impl.SetKeyframe(123, "123");

    AssertKeyframes_(impl,
                     new KeyframeDefinition<string>(2, "2"),
                     new KeyframeDefinition<string>(123, "123"),
                     new KeyframeDefinition<string>(1000, "1000")
    );
  }

  [Test]
  public void TestGetIndices() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(0, "first");
    impl.SetKeyframe(2, "second");
    impl.SetKeyframe(4, "third");

    Assert.AreEqual(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtIndex(0));
    Assert.AreEqual(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtIndex(1));
    Assert.AreEqual(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtIndex(2));
  }

  [Test]
  public void TestGetKeyframes() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(0, "first");
    impl.SetKeyframe(2, "second");
    impl.SetKeyframe(4, "third");

    AssertKeyframe_(new KeyframeDefinition<string>(0, default),
                    impl.GetKeyframeAtFrame(-1));
    AssertKeyframe_(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtFrame(0));
    AssertKeyframe_(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtFrame(1));
    AssertKeyframe_(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtFrame(2));
    AssertKeyframe_(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtFrame(3));
    AssertKeyframe_(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtFrame(4));
    AssertKeyframe_(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtFrame(5));
  }

  [Test]
  public void TestGetKeyframesReversed() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(4, "third");
    impl.SetKeyframe(2, "second");
    impl.SetKeyframe(0, "first");

    AssertKeyframe_(new KeyframeDefinition<string>(0, default),
                    impl.GetKeyframeAtFrame(-1));
    AssertKeyframe_(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtFrame(0));
    AssertKeyframe_(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtFrame(1));
    AssertKeyframe_(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtFrame(2));
    AssertKeyframe_(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtFrame(3));
    AssertKeyframe_(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtFrame(4));
    AssertKeyframe_(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtFrame(5));
  }

  [Test]
  public void TestGetKeyframesWeirdOrder() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(4, "third");
    impl.SetKeyframe(0, "first");
    impl.SetKeyframe(2, "second");

    AssertKeyframe_(new KeyframeDefinition<string>(0, default),
                    impl.GetKeyframeAtFrame(-1));
    AssertKeyframe_(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtFrame(0));
    AssertKeyframe_(new KeyframeDefinition<string>(0, "first"),
                    impl.GetKeyframeAtFrame(1));
    AssertKeyframe_(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtFrame(2));
    AssertKeyframe_(new KeyframeDefinition<string>(2, "second"),
                    impl.GetKeyframeAtFrame(3));
    AssertKeyframe_(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtFrame(4));
    AssertKeyframe_(new KeyframeDefinition<string>(4, "third"),
                    impl.GetKeyframeAtFrame(5));
  }

  [Test]
  public void TestFindIndexOfKeyframe() {
    var impl = new KeyframeDefinitions<string>();

    impl.SetKeyframe(0, "0");
    impl.SetKeyframe(1, "1");

    impl.FindIndexOfKeyframe(-1,
                             out var keyframeIndexMinus1,
                             out _,
                             out var isLastKeyframeMinus1);
    Assert.AreEqual(0, keyframeIndexMinus1);
    Assert.AreEqual(false, isLastKeyframeMinus1);

    impl.FindIndexOfKeyframe(0,
                             out var keyframeIndex0,
                             out _,
                             out var isLastKeyframe0);
    Assert.AreEqual(0, keyframeIndex0);
    Assert.AreEqual(false, isLastKeyframe0);

    impl.FindIndexOfKeyframe(1,
                             out var keyframeIndex1,
                             out _,
                             out var isLastKeyframe1);
    Assert.AreEqual(1, keyframeIndex1);
    Assert.AreEqual(true, isLastKeyframe1);

    impl.FindIndexOfKeyframe(2,
                             out var keyframeIndex2,
                             out _,
                             out var isLastKeyframe2);
    Assert.AreEqual(1, keyframeIndex2);
    Assert.AreEqual(true, isLastKeyframe2);
  }

  [Test]
  public void TestFindIndexOfKeyframeWhenEmpty() {
    var impl = new KeyframeDefinitions<string>();

    impl.FindIndexOfKeyframe(-1,
                             out var keyframeIndexMinus1,
                             out _,
                             out var isLastKeyframeMinus1);
    Assert.AreEqual(0, keyframeIndexMinus1);
    Assert.AreEqual(false, isLastKeyframeMinus1);

    impl.FindIndexOfKeyframe(0,
                             out var keyframeIndex0,
                             out _,
                             out var isLastKeyframe0);
    Assert.AreEqual(0, keyframeIndex0);
    Assert.AreEqual(false, isLastKeyframe0);
  }

  [Test]
  public void TestFindManyIndices() {
    var impl = new KeyframeDefinitions<string>();

    var s = 2;
    var n = 25;
    for (var sI = 0; sI < n; sI += s) {
      impl.SetKeyframe(sI, $"{sI}");
    }

    for (var i = 0; i < n; ++i) {
      var sI = i - (i % s);

      var isKeyframeDefined = impl.FindIndexOfKeyframe(i,
        out var keyframeIndex,
        out var keyframe,
        out var isLastKeyframe);

      Assert.IsTrue(isKeyframeDefined);
      Assert.AreEqual(sI / s, keyframeIndex);
      Assert.AreEqual(sI, keyframe.Frame);
      Assert.AreEqual(i == n - 1, isLastKeyframe);
    }
  }

  private static void AssertKeyframe_(KeyframeDefinition<string>? expected,
                                      KeyframeDefinition<string>? actual)
    => Assert.AreEqual(expected, actual);

  private static void AssertKeyframes_(KeyframeDefinitions<string> actual,
                                       params KeyframeDefinition<string>[]
                                           expected)
    => Asserts.SequenceEqual(expected, actual.Definitions);
}