using GDLibrary.Components;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace Tests_GD3_2021_GDLibrary
{
    public class TransformTests
    {
        [Test]
        public void TestClone()
        {
            var transform = new Transform();
            var clone = transform.Clone() as Transform;

            Assert.NotNull(clone);
            Assert.AreNotSame(clone, transform);
        }

        [Test]
        public void TestTranslate()
        {
            var transform = new Transform();
            transform.Translate(1, 2, 3);
            Assert.AreEqual(transform.LocalTranslation, new Vector3(1, 2, 3));
        }

        [Test]
        public void TestRotate()
        {
            var transform = new Transform();
            transform.Rotate(45, -90, 360);
            Assert.AreEqual(transform.LocalRotation, new Vector3(45, -90, 360));
        }

        [Test]
        public void TestScale()
        {
            var transform = new Transform();
            transform.Scale(4, -8, 0.5f);
            Assert.AreEqual(transform.LocalScale, new Vector3(5, -7, 1.5f));
        }

        [Test]
        public void TestSetTranslation()
        {
            var transform = new Transform();
            transform.SetTranslation(1, 2, 3);
            Assert.AreEqual(transform.LocalTranslation, new Vector3(1, 2, 3));
        }

        [Test]
        public void TestSetRotation()
        {
            var transform = new Transform();
            transform.SetRotation(45, -90, 360);
            Assert.AreEqual(transform.LocalRotation, new Vector3(45, -90, 360));
        }

        [Test]
        public void TestSetScale()
        {
            var transform = new Transform();
            transform.SetScale(4, -8, 0.5f);
            Assert.AreEqual(transform.LocalScale, new Vector3(4, -8, 0.5f));
        }

        [Test]
        public void TestDeepCloneTrans()
        {
            var transform = new Transform();
            var clone = transform.Clone() as Transform;

            clone.SetTranslation(1, 2, 3);
            Assert.AreNotEqual(clone.LocalTranslation, transform.LocalTranslation);
        }

        [Test]
        public void TestDeepCloneRot()
        {
            var transform = new Transform();
            var clone = transform.Clone() as Transform;

            clone.SetRotation(45, -90, 360);
            Assert.AreNotEqual(clone.LocalRotation, transform.LocalRotation);
        }

        [Test]
        public void TestDeepCloneScale()
        {
            var transform = new Transform();
            var clone = transform.Clone() as Transform;

            clone.SetScale(4, -8, 0.5f);
            Assert.AreNotEqual(clone.LocalScale, transform.LocalScale);
        }

        [Test]
        public void TestDeepCloneWorldEqual()
        {
            var transform = new Transform();
            var clone = transform.Clone() as Transform;

            //manually call Update() to force world matrix
            clone.Update();
            transform.Update();

            Assert.AreEqual(clone.WorldMatrix, transform.WorldMatrix);
        }

        [Test]
        public void TestDeepCloneWorldDifferent()
        {
            var transform = new Transform();
            var clone = transform.Clone() as Transform;

            clone.SetTranslation(1, 2, 3);

            //manually call Update() to force world matrix
            clone.Update();
            transform.Update();

            var cloneWorld = clone.WorldMatrix;
            var origWorld = transform.WorldMatrix;

            Assert.AreNotEqual(cloneWorld, origWorld);
        }
    }
}