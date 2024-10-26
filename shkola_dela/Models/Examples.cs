using Swashbuckle.AspNetCore.Filters;

namespace shkola_dela.Models;

public class ClientPOST
{
    public class ClientExample
    {
        public string Inn { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<int> FounderIds { get; set; }
    }

    public class CreateClientResponse
    {
        public int Id { get; set; }
        public string Inn { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
        public List<int> FounderIds { get; set; }
    }

    public class CreateClientRequestExample : IExamplesProvider<ClientExample>
    {
        public ClientExample GetExamples()
        {
            return new ClientExample
            {
                Inn = "7727563778",
                Name = "Юрлицо №1",
                Type = "LegalPerson",
                FounderIds = [1, 2]
            };
        }
    }

    public class CreateClientResponseExample : IExamplesProvider<CreateClientResponse>
    {
        public CreateClientResponse GetExamples()
        {
            return new CreateClientResponse
            {
                Id = 1,
                Inn = "7727563778",
                Name = "Юрлицо №1",
                Type = "LegalPerson",
                DateAdded = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                FounderIds = [1, 2]
            };
        }
    }
}


public class ClientGET
{
    public class ClientResponse
    {
        public int Id { get; set; }
        public string Inn { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
        public List<int> FounderIds { get; set; }
    }

    public class ClientResponseListExample : IExamplesProvider<List<ClientResponse>>
    {
        public List<ClientResponse> GetExamples()
        {
            return new List<ClientResponse>
            {
                new ClientResponse
                {
                    Id = 1,
                    Inn = "7727563778",
                    Name = "Юрлицо №1",
                    Type = "LegalPerson",
                    DateAdded = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow,
                    FounderIds = new List<int> { 1, 2 }
                }
            };
        }
    }
}

public class ClientGETID
{
    public class ClientResponse
    {
        public int Id { get; set; }
        public string Inn { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
        public List<int> FounderIds { get; set; }
    }

    public class ClientResponseExample : IExamplesProvider<ClientResponse>
    {
        public ClientResponse GetExamples()
        {
            return new ClientResponse
            {
                Id = 1,
                Inn = "7727563778",
                Name = "Юрлицо №1",
                Type = "LegalPerson",
                DateAdded = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                FounderIds = [1, 2]
            };
        }
    }
}