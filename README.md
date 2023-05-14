# Traceless

Traceless is a privacy-centric chat application built with Microsoft's Blazor server technology. It is designed to be highly scalable, capable of handling a large number of users without compromising performance or user experience.

Unlike traditional chat applications that store conversation data indefinitely, Traceless respects the ephemeral nature of conversations by storing chat history for only five days, after which it is permanently deleted. This approach not only preserves user privacy but also enhances the application's scalability by minimizing long-term data storage requirements.

Whether you're communicating one-on-one or collaborating in large groups, Traceless is engineered to scale smoothly and efficiently. This scalability, combined with our commitment to privacy, makes Traceless an ideal choice for anyone seeking a secure, user-friendly, and scalable chat application.

## Scalability
Traceless is built with scalability in mind. Whether it's accommodating new users or handling increased message volume, our application is designed to scale smoothly to meet growing demands. By minimizing long-term data storage and leveraging efficient server-side rendering technology, Traceless provides a high-performance chat experience even as user activity grows. Whether your community is just a handful of people or hundreds of thousands, Traceless is engineered to deliver reliable, fast, and secure service.

## Features
- Short-Term Message Storage: Chat history is stored for only five days, reducing the risk of long-term data exposure.
- User-Friendly Interface: Built with Microsoft's Blazor server technology for a smooth, seamless user experience.
- Group and Direct Chats: Enjoy group chats, direct messages, and media sharing capabilities.
- Data Encryption: Robust encryption for both data at rest and in transit ensures comprehensive data protection.

## Installation

To get started with Traceless, follow these steps with Docker:


```bash
git clone https://github.com/iamhyunsoo/Traceless.git
cd Traceless
```

#### MUST update the ConnectionStrings in Server/appsettings.json
```json
"ConnectionStrings": {
  "DefaultConnection": "USE YOUR MYSQL CONNECTION STRING",
  "MyRedisConStr": "USE YOUR REDIS ENDPOINT"
}

```

If you have to hard code AWS credentials, update codes in Server/Program.cs and Server/appsettings.json with your AWS credentials
```cs
var awsOptions = configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
builder.Services.AddSingleton<IDynamoDBContext>(sp => new DynamoDBContext(sp.GetRequiredService<IAmazonDynamoDB>()));
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.USEast2));
```
```json
"AWS": {
  "AccessKey": "your-access-key",
  "SecretKey": "your-secret-key"
}
```

Build a Docker image and run it
```bash
docker build -t traceless .
docker run --rm -p 80:80 --name traceless-app traceless:latest
```



## Usage

1. Register an account
2. Login with your credentials
3. Start a new chat or join an existing one
4. Enjoy secure, private conversations that are traceless after five days!

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)
