﻿using System;
using Moq;
using CDP.Core;

namespace CDP.Core.Tests
{
    class MockProvider<T> : IObjectProvider<T> where T:class
    {
        public Mock<T> Mock { get; private set; }
        private bool dirty = false;

        public MockProvider()
        {
            Mock = new Mock<T>();
        }

        public T Get(params object[] args)
        {
            if (dirty)
            {
                throw new InvalidOperationException("This MockProvider has already been used.");
            }

            dirty = true;
            return Mock.Object;
        }
    }
}