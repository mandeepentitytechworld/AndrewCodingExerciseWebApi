using Andrew.Models;
using Andrew.Services.Intrefaces;
using AndrewCodingExerciseWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;

namespace AndrewCodingExercise.Test
{
    public class ProcessPaymentUnitTest
    {
        PaymentController _controller;
        IProcessPaymentService _service;

        public ProcessPaymentUnitTest()
        {
            _service = new ProcessPaymentServiceFake();
            _controller = new PaymentController(_service);
        }

        [Fact]
        public async void Task_Add_ValidObjectPassed_ReturnsCreatedResponse()
        {
            // Arrange
            ProcessPaymentViewModel testItem = new ProcessPaymentViewModel()
            {
                CreditCardNumber = "0001000000000000",
                CardHolderName = "Red",
                ExpiryDate = "01/22",
                CVV = "000",
                Amount = 20
            };
            // Act
            var createdResponse = await _controller.ProcessPayment(testItem);
            // Assert
            Assert.IsType<ActionResult<ProcessPaymentViewModel>>(createdResponse);
        }
    }
}
