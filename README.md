My Main Project:
Game Back-End Microservice Architecture
This project is a microservice-based architecture for a game back-end, built using ASP.NET Core with Web API, MediatR, SignalR, Entity Framework, MSSQL, MongoDB, Docker, and Kubernetes. It utilizes gRPC for inter-service communication and RabbitMQ for message queuing.
Each service follows a clean architecture design, ensuring scalability, maintainability, and efficient inter-service interaction.

Technologies Used
ASP.NET Core: For building web APIs
Entity Framework & MSSQL: Database management
MongoDB: Storing game session data
Docker: Containerization of services
Kubernetes: Service orchestration
gRPC: Efficient inter-service communication
RabbitMQ: Event-driven messaging
SignalR: Real-time notifications
Ingress NGINX: Centralized host access for services
Services Overview


Player Service

Responsibilities: Manages player authentication and authorization using JWT tokens, implemented with Microsoft Identity. It provides endpoints for players to joining and leaving matchmaking queues.
Integration: Communicates with the Game Domain Service via gRPC to validate game modes before adding players to matchmaking. On joining, it sends an event to RabbitMQ, which the Matchmaking Service consumes.

Matchmaking Service

Responsibilities: Processes matchmaking events from RabbitMQ, maintaining queues of players waiting for games. Queues are structured using a ConcurrentDictionary, with regions as keys and another ConcurrentDictionary gamemodes as sub-keys and list of players as queue of players on specific region on specific gamemode.
Functionality:
Starts by querying the Game Domain Service via gRPC to retrieve available game modes, which are stored in a HashSet.
Monitors queues to check if the player count for a specific region and game mode reaches the maximum allowed players. If so, it generates a CreateGame event.
Consumes NewGameModeCreated and NewRegionCreated events from RabbitMQ, updating its queues to include the new data.

Game Session Service

Responsibilities: Manages active game sessions, with both API endpoints and background services. It catches GameCreated events, initializes game sessions, and stores data in MongoDB.
Key Features:
KillAction Endpoint: Records player actions, triggering events processed by a monitoring service. If certain conditions are met (e.g., all players on a team are eliminated), it ends the game round and sends real-time notifications using SignalR.
Session Monitoring: Tracks game progress, checks if rounds have reached the maximum score, and handles end-of-game scenarios. It also communicates with the Player Service via gRPC to update player stats, including money and levels.
Player Stats Endpoint: Provides real-time feedback to players on their in-game stats through SignalR.

Game Domain Service

Responsibilities: Stores essential game configuration data, such as game modes (name, maximum players, max score) and available regions.
Integration: Acts as a gRPC server, providing game configuration information to other services. Admins can manage game modes and regions via API endpoints, triggering events (GameModeCreated, RegionCreated) that the Matchmaking Service consumes.

Security and Access Control

JWT Authentication: All service endpoints, except matchmaking,  require JWT tokens, obtained from the Player Service.
Role-based Authorization: Configured to support different user roles, ensuring secure access across services.

Infrastructure and Deployment

Containerization: Each service is containerized using Docker, enabling easy deployment and scaling.
Kubernetes Orchestration: Services are orchestrated with Kubernetes, using YAML files stored in the K8S folder for configuration.

Ingress NGINX: Centralized host access, allowing seamless communication between services and external clients.

RabbitMQ & MSSQL: Configured with load balancers for high availability and efficiency, with ClusterIPs specified in service deployment files.
Key Highlights

Scalability: Designed to scale, with independent services that can be deployed, updated, and scaled individually.

Event-Driven Architecture: RabbitMQ facilitates seamless communication between services, ensuring real-time updates and efficient processing.

Real-Time Notifications: SignalR provides players with real-time feedback, enhancing the gaming experience.   
