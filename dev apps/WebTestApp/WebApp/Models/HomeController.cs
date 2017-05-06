﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private const string msGraphQuery = "https://graph.microsoft.com/v1.0/me";

        private const string msGraphScope = "https://graph.microsoft.com/.default";


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View("~/Views/Shared/Error.cshtml");
        }
        [HttpGet]
        public async Task<IActionResult> CallGraph()
        {
            var preferred_username = (User.FindFirst("preferred_username"))?.Value;

            var user = Startup.confidentialClient.Users.FirstOrDefault(u => u.DisplayableId.Equals(preferred_username));
            var r = await Startup.confidentialClient.AcquireTokenSilentAsync(Startup.scopes, user);


            // Query for list of users in the tenant
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, msGraphQuery);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", r.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.StatusCode.ToString());
            }

            // Record users in the data store (note that this only records the first page of users)
            string json = await response.Content.ReadAsStringAsync();
           // MsGraphUserListResponse users = JsonConvert.DeserializeObject<MsGraphUserListResponse>(json);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CallGraphClientCredential()
        {
            AuthenticationResult r = null;
            try
            {
                r = await Startup.confidentialClient.AcquireTokenForClientAsync(new string[] { msGraphScope });
            }
            catch (Exception e)
            {

            }

            // Query for list of users in the tenant
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, msGraphQuery);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", r.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.StatusCode.ToString());
            }

            // Record users in the data store (note that this only records the first page of users)
            string json = await response.Content.ReadAsStringAsync();
            // MsGraphUserListResponse users = JsonConvert.DeserializeObject<MsGraphUserListResponse>(json);

            return View();
        }
    }
}
