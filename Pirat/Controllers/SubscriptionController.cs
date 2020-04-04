﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pirat.Codes;
using Pirat.Exceptions;
using Pirat.Extensions.Swagger.SwaggerConfiguration;
using Pirat.Model;
using Pirat.Services;
using Pirat.Services.Mail;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Pirat.Controllers
{
    [ApiController]
    [Route("/subscription")]
    public class SubscriptionController: ControllerBase
    {
        private readonly IMailService _mailService;

        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(IMailService mailService, ISubscriptionService subscriptionService)
        {
            _mailService = mailService;
            _subscriptionService = subscriptionService;
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [SwaggerRequestExample(typeof(RegionSubscription), typeof(RegionSubscriptionExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(string))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(EmptyResponseExample))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErrorCodeResponseExample))]
        public IActionResult Post([FromBody] RegionSubscription regionsubscription)
        {
            if (!_mailService.verifyMail(regionsubscription.email))
            {
                return BadRequest(Error.ErrorCodes.INVALID_MAIL);
            }
            try
            {
                this._subscriptionService.SubscribeRegion(regionsubscription);
                this._mailService.sendRegionSubscriptionConformationMail(regionsubscription);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (UnknownAdressException e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
    }
}
