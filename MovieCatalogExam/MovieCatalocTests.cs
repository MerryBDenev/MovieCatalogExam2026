using System;
using System.Data;
using System.Net;
using System.Text.Json;
using Microsoft.VisualBasic;
using RestSharp;
using RestSharp.Authenticators;
using MovieCatalogExam.Models;


namespace MovieCatalogExam
{
    public class Tests
    {
        private RestClient client;
        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticTocken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2MzM3OTBhYi03M2NjLTQzNWUtOWE1NS0yMmYyZjhmMDEzMzUiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjA1OjE3IiwiVXNlcklkIjoiYmY5MDc2ZTctOTY5Mi00ZWEzLTYyMjAtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJNZXJyeWJEMkBkbXguY29tIiwiVXNlck5hbWUiOiJNZXI0eUJkIiwiZXhwIjoxNzc2NTEzOTE3LCJpc3MiOiJNb3ZpZUNhdGFsb2dfQXBwX1NvZnRVbmkiLCJhdWQiOiJNb3ZpZUNhdGFsb2dfV2ViQVBJX1NvZnRVbmkifQ.PdwOoYl6m-2KlUKT-kPYdFQClf7hUoamWixajbZ6EWo";
        private const string LoginEmail = "MerrybD2@dmx.com";
        private const string LoginPassword = "123456789";
        private static string movieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;
            if (!string.IsNullOrWhiteSpace(StaticTocken))
            {
                jwtToken = StaticTocken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestRequest(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            var response = new RestClient(BaseUrl).Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateMovieWithRequiredFields_ShouldReturnSuccess()
        {
            MovieDTO newMovie = new MovieDTO
            {
                Title = "Super Mario",
                Description = "This is a new Movie.",
                PosterUrl = "",
                TrailerUrl = "",
                IsWatched = true
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            RestResponse response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Movie, Is.Not.Null); 
            Assert.That(readyResponse.Movie.Id, Is.Not.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));
            movieId = readyResponse.Movie.Id;
        }

        [Order(2)]
        [Test]
        public void EditLstCreatedMovie_ShouldReturnSuccess()
        {
            var editLastMovie = new MovieDTO
            {
                Title = "Not a Super Mario",
                Description = "This is a new Movie.",
            };

            RestRequest request = new RestRequest($"/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", movieId);
            request.AddJsonBody(editLastMovie);

            var response = client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));            
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnListWithAllMovies()
        {
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
            RestResponse response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            List<MovieDTO> movies = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);
            Assert.That(movies, Is.Not.Null);
            Assert.That(movies, Is.Not.Empty);
            Assert.That(movies.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void DeleteLastMovie_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete/", Method.Delete);

            request.AddQueryParameter("movieId", movieId);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovieWithoutRequeredFields_ShouldReturnBadRequest()
        {
            MovieDTO newIncorrectMovie = new MovieDTO
            {
                Title = "",
                Description = "",
                PosterUrl = "",
                TrailerUrl = "",
                IsWatched = true
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(newIncorrectMovie);

            RestResponse response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));    
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "456";

            MovieDTO editNonExistingMovie = new MovieDTO
            {
                Title = "Not a Super Mario",
                Description = "This is a new Movie.",
            };
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(editNonExistingMovie);

            RestResponse response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));      
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "456";
            RestRequest request = new RestRequest($"/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", nonExistingMovieId);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

            [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }

    }
}