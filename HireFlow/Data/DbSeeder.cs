using HireFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Data
{
    public static class DbSeeder
    {
        public static async Task SeedJobsAsync(ApplicationDbContext context)
        {
            // Ensure database is ready
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Realistic IT Companies & Recruiters if missing
            var recruiters = new List<(string Email, string Name)>
            {
                ("talent.google@hireflow.io", "Google Cloud Engineering"),
                ("careers.microsoft@hireflow.io", "Microsoft Azure Platform"),
                ("tech.amazon@hireflow.io", "Amazon AWS Core Services"),
                ("jobs.stripe@hireflow.io", "Stripe Global Payments"),
                ("talent.meta@hireflow.io", "Meta AI & Infrastructure"),
                ("careers.netflix@hireflow.io", "Netflix Streaming Systems"),
                ("talent.swiggy@hireflow.io", "Swiggy Core Consumer Tech"),
                ("tech.razorpay@hireflow.io", "Razorpay Fintech Platform"),
                ("jobs.atlassian@hireflow.io", "Atlassian Cloud Tools"),
                ("careers.nvidia@hireflow.io", "NVIDIA Deep Learning & GPU"),
                ("talent.snowflake@hireflow.io", "Snowflake Data Cloud"),
                ("careers.tcs@hireflow.io", "TCS Digital Enterprise"),
                ("jobs.infosys@hireflow.io", "Infosys NextGen Labs"),
                ("careers.flipkart@hireflow.io", "Flipkart Commerce Systems")
            };

            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Recruiter@123");
            var recruiterMap = new Dictionary<string, int>();

            foreach (var (email, name) in recruiters)
            {
                var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existing == null)
                {
                    var user = new User
                    {
                        FullName = name,
                        Email = email,
                        PasswordHash = defaultPasswordHash,
                        Role = "Recruiter",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3)
                    };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    recruiterMap[email] = user.Id;
                }
                else
                {
                    recruiterMap[email] = existing.Id;
                }
            }

            // Fallback recruiter ID if needed
            var fallbackRecruiterId = recruiterMap.Values.FirstOrDefault();
            if (fallbackRecruiterId == 0)
            {
                var anyRecruiter = await context.Users.FirstOrDefaultAsync(u => u.Role == "Recruiter");
                if (anyRecruiter != null)
                {
                    fallbackRecruiterId = anyRecruiter.Id;
                }
                else
                {
                    var newRecruiter = new User
                    {
                        FullName = "HireFlow Tech Talent Partners",
                        Email = "partner@hireflow.io",
                        PasswordHash = defaultPasswordHash,
                        Role = "Recruiter",
                        IsActive = true
                    };
                    context.Users.Add(newRecruiter);
                    await context.SaveChangesAsync();
                    fallbackRecruiterId = newRecruiter.Id;
                }
            }

            // 2. Check if we already have 50+ jobs
            var currentJobCount = await context.Jobs.CountAsync();
            if (currentJobCount >= 50)
            {
                return; // Already populated
            }

            // 3. Define 55+ Authentic, In-Depth Real IT Jobs
            int GetRecruiter(string key) => recruiterMap.TryGetValue(key, out var id) ? id : fallbackRecruiterId;

            var itJobs = new List<Job>
            {
                // ==========================================
                // 1. FRONTEND ENGINEERING (1 - 7)
                // ==========================================
                new Job
                {
                    Title = "Senior React & Next.js Engineer",
                    Description = "Architect modern responsive web applications using React 19, Next.js App Router, TypeScript, and Tailwind CSS. Focus on Core Web Vitals, SSR performance, and clean component systems.",
                    Location = "Remote / Bangalore",
                    JobType = "Remote",
                    Salary = "₹22,00,000 - ₹34,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.stripe@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Lead Frontend Architect (Design Systems & Microfrontends)",
                    Description = "Drive enterprise UI architecture across distributed teams. Define design tokens, microfrontend federation with Webpack/Vite, accessibility (WCAG AA), and headless component architecture.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹35,00,000 - ₹50,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.atlassian@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "Staff Vue.js / Nuxt 3 Developer",
                    Description = "Build high-speed customer-facing analytics dashboards using Vue 3 Composition API, Pinia, TypeScript, and Vite. Collaborate closely with product managers and backend API teams.",
                    Location = "Hyderabad, India",
                    JobType = "Full Time",
                    Salary = "₹24,00,000 - ₹36,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.flipkart@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Angular 18 Enterprise Developer",
                    Description = "Develop mission-critical banking and fintech portals with Angular 18, RxJS signals, NgRx state management, and high-performance tables handling millions of live records.",
                    Location = "Pune / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹16,00,000 - ₹26,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.razorpay@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Job
                {
                    Title = "Junior Web Developer (React & TypeScript)",
                    Description = "Great entry-level opportunity for passionate frontend engineers. You will build user interfaces, write unit tests with Jest/Vitest, and integrate RESTful APIs in an agile team.",
                    Location = "Noida, India",
                    JobType = "Full Time",
                    Salary = "₹6,50,000 - ₹10,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.infosys@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Web3 & dApp Frontend Engineer",
                    Description = "Construct decentralized application interfaces using Ethers.js, Wagmi, Next.js, and RainbowKit. Experience with smart contract wallet connections and event listeners required.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$110,000 - $145,000 / yr",
                    RecruiterId = GetRecruiter("jobs.stripe@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Job
                {
                    Title = "UI/UX Creative Frontend Developer (Three.js & Canvas)",
                    Description = "Bring rich 3D interactions, WebGL shaders, Canvas animations, and micro-interactions to life for award-winning marketing and product showcase experiences.",
                    Location = "Remote / Mumbai",
                    JobType = "Contract",
                    Salary = "₹18,00,000 - ₹28,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.swiggy@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },

                // ==========================================
                // 2. BACKEND & DISTRIBUTED SYSTEMS (8 - 15)
                // ==========================================
                new Job
                {
                    Title = "Senior .NET Core / C# Microservices Architect",
                    Description = "Design and scale distributed microservices with ASP.NET Core 9/10, Entity Framework Core, MassTransit/RabbitMQ, and SQL Server/PostgreSQL. Deploy on Docker & Kubernetes.",
                    Location = "Hyderabad, India",
                    JobType = "Full Time",
                    Salary = "₹26,00,000 - ₹40,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.microsoft@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Job
                {
                    Title = "Staff Java / Spring Boot Backend Engineer",
                    Description = "Lead the design of ultra-high-throughput payment engines handling 50k+ transactions per second. Deep expertise in Java 21, Spring Boot 3, Kafka, Redis clustering, and distributed caching.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹36,00,000 - ₹52,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.razorpay@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Golang High-Concurrency Distributed Systems Engineer",
                    Description = "Build low-latency streaming infrastructure and cloud-native services in Go (Golang). Heavy focus on goroutines, gRPC, Protobuf, RocksDB, and Kubernetes operator development.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$135,00,00 - $175,000 / yr",
                    RecruiterId = GetRecruiter("talent.google@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Python / FastAPI Backend Engineer (AI Integrations)",
                    Description = "Develop performant async REST & GraphQL APIs powering generative AI agents. Hands-on experience with FastAPI, Pydantic v2, Celery, LangChain/LlamaIndex, and Vector DBs.",
                    Location = "Gurugram / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹20,00,000 - ₹32,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.meta@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "Node.js & TypeScript Backend Lead",
                    Description = "Manage and scale real-time backend microservices using Node.js, NestJS, TypeScript, WebSocket, and PostgreSQL. Implement OAuth2, RBAC security, and event-driven architectures.",
                    Location = "Bangalore / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹28,00,000 - ₹42,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.swiggy@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "C++ Low-Latency Systems Software Engineer",
                    Description = "Develop high-frequency trading (HFT) matching engines and networking stack in modern C++ (C++20). Work on memory optimization, kernel bypass (Solarflare), and sub-microsecond latency.",
                    Location = "Mumbai, India",
                    JobType = "Full Time",
                    Salary = "₹45,00,000 - ₹75,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.tcs@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Job
                {
                    Title = "Rust Systems & Cloud Infrastructure Engineer",
                    Description = "Contribute to core cloud storage and virtualization engines written in Rust. Requires deep understanding of memory safety, async Tokio, low-level OS primitives, and WebAssembly.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$140,000 - $185,000 / yr",
                    RecruiterId = GetRecruiter("careers.netflix@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Job
                {
                    Title = "Ruby on Rails Senior Backend Developer",
                    Description = "Scale an established SaaS platform servicing millions of daily active users. Expert in Ruby 3, Rails 7+, Sidekiq background jobs, Redis caching, and PostgreSQL query tuning.",
                    Location = "Remote / Pune",
                    JobType = "Remote",
                    Salary = "₹18,00,000 - ₹28,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.atlassian@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                },

                // ==========================================
                // 3. FULL STACK ENGINEERING (16 - 21)
                // ==========================================
                new Job
                {
                    Title = "Senior Full Stack Engineer (MERN Stack)",
                    Description = "Deliver end-to-end features spanning React frontend, Node.js/Express backend, and MongoDB database. Strong background in RESTful design, automated testing, and CI/CD pipelines.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "₹20,00,000 - ₹30,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.flipkart@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Principal Full Stack Engineer (.NET Core + React)",
                    Description = "Lead technical execution across full stack products. Architect scalable C# APIs, maintain sleek React/TypeScript web apps, and mentor engineering pods in microservice best practices.",
                    Location = "Bangalore / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹32,00,000 - ₹48,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.microsoft@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Full Stack Java + Angular Engineer",
                    Description = "Build and maintain enterprise cloud services using Java 17, Spring Boot, Angular 17, Oracle DB, and Docker. Experience with enterprise identity systems and SAML/OIDC is a plus.",
                    Location = "Chennai, India",
                    JobType = "Full Time",
                    Salary = "₹15,00,000 - ₹24,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.infosys@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Job
                {
                    Title = "Full Stack Python + Vue Cloud Developer",
                    Description = "Develop data visualization tools and analytical dashboards using Python (Django/FastAPI) and Vue.js. Integrate with AWS services (S3, Lambda, RDS) and automated testing suites.",
                    Location = "Hyderabad, India",
                    JobType = "Full Time",
                    Salary = "₹18,00,000 - ₹27,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.snowflake@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Full Stack Developer (Next.js & Supabase)",
                    Description = "Fast-paced startup engineering role. Ship new product iterations rapidly with Next.js 14, Tailwind, Supabase (PostgreSQL), and serverless edge functions on Vercel.",
                    Location = "Remote",
                    JobType = "Contract",
                    Salary = "$80,000 - $110,000 / yr",
                    RecruiterId = GetRecruiter("jobs.stripe@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "Graduate Full Stack Trainee (Batch 2025/2026)",
                    Description = "Launch your tech career with structured mentorship. Learn and build software with React, Node.js, C#, and relational databases. Open to fresh graduates with strong CS fundamentals.",
                    Location = "Noida / Bangalore",
                    JobType = "Internship",
                    Salary = "₹5,00,000 - ₹7,50,000 / yr",
                    RecruiterId = GetRecruiter("careers.tcs@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // ==========================================
                // 4. CLOUD, DEVOPS, SRE & PLATFORM (22 - 28)
                // ==========================================
                new Job
                {
                    Title = "Senior DevOps Engineer (AWS & Terraform)",
                    Description = "Automate multi-region cloud infrastructure using Terraform, AWS (EKS, VPC, CloudFront, RDS), and GitHub Actions. Implement GitOps with ArgoCD and Helm chart packaging.",
                    Location = "Remote / Bangalore",
                    JobType = "Remote",
                    Salary = "₹25,00,000 - ₹38,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.amazon@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Lead Site Reliability Engineer (SRE) - 99.99% SLA",
                    Description = "Champion system reliability and incident management. Monitor mission-critical systems with Prometheus, Grafana, Datadog, and OpenTelemetry. Build automated failover & chaos testing.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹34,00,000 - ₹50,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.netflix@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Azure Cloud Infrastructure Architect",
                    Description = "Plan enterprise migrations to Microsoft Azure. Implement Azure Landing Zones, ExpressRoute, Azure Kubernetes Service (AKS), cost optimization (FinOps), and policy governance.",
                    Location = "Noida / Gurugram",
                    JobType = "Full Time",
                    Salary = "₹30,00,000 - ₹46,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.microsoft@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Job
                {
                    Title = "Kubernetes & Platform Engineer",
                    Description = "Design internal developer platforms (IDP) with Kubernetes, Backstage, Istio service mesh, Vault secrets management, and automated cluster lifecycle provisioning.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$130,000 - $165,000 / yr",
                    RecruiterId = GetRecruiter("talent.google@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "Cloud Security & DevSecOps Engineer",
                    Description = "Integrate security into CI/CD pipelines (SAST, DAST, SCA, Container scanning via Trivy/Snyk). Manage IAM roles, secret rotation, and AWS/Azure compliance guardrails.",
                    Location = "Hyderabad, India",
                    JobType = "Full Time",
                    Salary = "₹24,00,000 - ₹36,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.razorpay@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Linux Systems Administrator (RedHat / Ubuntu)",
                    Description = "Manage 2,000+ bare-metal and virtual Linux servers. Script routine maintenance with Bash and Python, maintain Ansible playbooks, and optimize kernel network parameters.",
                    Location = "Mumbai, India",
                    JobType = "Full Time",
                    Salary = "₹12,00,000 - ₹18,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.tcs@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Job
                {
                    Title = "DevOps & Cloud Engineer Intern",
                    Description = "Learn and practice cloud automation. Work alongside senior architects to build Docker containers, automate CI/CD scripts with GitHub Actions, and monitor cloud logs.",
                    Location = "Pune, India",
                    JobType = "Internship",
                    Salary = "₹40,000 - ₹60,000 / mo",
                    RecruiterId = GetRecruiter("jobs.infosys@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },

                // ==========================================
                // 5. AI, MACHINE LEARNING & DATA (29 - 36)
                // ==========================================
                new Job
                {
                    Title = "Senior Machine Learning Engineer (LLMs & Fine-Tuning)",
                    Description = "Train and fine-tune large language models (LLMs) with PyTorch, Hugging Face Transformers, LoRA, and vLLM. Deploy high-throughput inference endpoints on GPU clusters.",
                    Location = "Remote / Bangalore",
                    JobType = "Remote",
                    Salary = "₹35,00,000 - ₹55,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.nvidia@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Job
                {
                    Title = "Staff Data Platform Engineer (Snowflake & dbt)",
                    Description = "Build modern data stack architectures. Design petabyte-scale ELT pipelines with Snowflake, dbt core, Apache Airflow, Kafka, and data quality testing frameworks.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹30,00,000 - ₹48,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.snowflake@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "AI Research Scientist (Computer Vision & Multimodal)",
                    Description = "Conduct original research in generative image/video synthesis and multimodal representations. Strong publication record (CVPR, ICCV, NeurIPS) and mastery of PyTorch/CUDA required.",
                    Location = "Bangalore / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹45,00,000 - ₹70,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.meta@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "MLOps Engineer (Kubeflow & Model Deployment)",
                    Description = "Bridge machine learning research and production. Implement model monitoring, drift detection, feature stores (Feast), Kubeflow pipelines, and Triton inference servers.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$125,000 - $160,000 / yr",
                    RecruiterId = GetRecruiter("careers.nvidia@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Big Data Engineer (Apache Spark & Kafka)",
                    Description = "Ingest, process, and analyze streaming event logs in real-time. Hands-on expertise in PySpark/Scala, Apache Flink, Kafka streams, Delta Lake, and AWS EMR.",
                    Location = "Hyderabad, India",
                    JobType = "Full Time",
                    Salary = "₹22,00,000 - ₹34,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.flipkart@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Job
                {
                    Title = "Senior Business Intelligence & Power BI Analyst",
                    Description = "Transform complex product data into executive dashboards using Power BI, DAX, SQL, and Tableau. Collaborate with business heads to deliver actionable conversion insights.",
                    Location = "Gurugram, India",
                    JobType = "Full Time",
                    Salary = "₹14,00,000 - ₹22,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.swiggy@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Job
                {
                    Title = "Data Science Associate (Predictive Modeling & NLP)",
                    Description = "Develop statistical algorithms for churn prediction, demand forecasting, and customer sentiment analysis using Python, Scikit-learn, XGBoost, and SQL.",
                    Location = "Pune / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹12,00,000 - ₹19,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.infosys@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Job
                {
                    Title = "Generative AI Prompt Engineer & Evaluator",
                    Description = "Design benchmark evaluation suites for enterprise LLM outputs. Optimize system prompts, implement RAG accuracy metrics, and build red-teaming test cases.",
                    Location = "Remote",
                    JobType = "Contract",
                    Salary = "$70,000 - $95,000 / yr",
                    RecruiterId = GetRecruiter("talent.meta@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },

                // ==========================================
                // 6. CYBERSECURITY & NETWORK (37 - 42)
                // ==========================================
                new Job
                {
                    Title = "Senior Information Security Analyst (SOC Tier 3)",
                    Description = "Lead threat hunting, incident response, and forensic investigations. Proficient in Splunk, Microsoft Sentinel, MITRE ATT&CK framework, and endpoint detection (CrowdStrike).",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹20,00,000 - ₹32,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.tcs@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "Lead Penetration Tester & Ethical Hacker",
                    Description = "Conduct black-box, grey-box, and white-box security assessments on web, mobile, and cloud environments. OSCP, CRTP, or CEH certification highly desired.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "₹24,00,000 - ₹38,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.razorpay@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Application Security (AppSec) Specialist",
                    Description = "Embed security into developer workflows. Review code for OWASP Top 10 vulnerabilities, conduct threat modeling, and manage responsible disclosure bug bounty programs.",
                    Location = "Hyderabad / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹26,00,000 - ₹40,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.stripe@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Network Security Engineer (Firewalls & Zero Trust)",
                    Description = "Configure and manage Palo Alto next-gen firewalls, Cisco SD-WAN, Fortinet, and Zero-Trust network architectures across multi-site global offices.",
                    Location = "Noida, India",
                    JobType = "Full Time",
                    Salary = "₹15,00,000 - ₹24,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.infosys@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Job
                {
                    Title = "Cloud Security Architect (AWS & GCP)",
                    Description = "Formulate global cloud security policies, KMS encryption key management, SCPs, GuardDuty findings remediation, and CIS benchmarks implementation.",
                    Location = "Remote / Bangalore",
                    JobType = "Remote",
                    Salary = "₹32,00,000 - ₹48,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.amazon@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Job
                {
                    Title = "Cybersecurity Governance, Risk & Compliance (GRC) Lead",
                    Description = "Manage SOC2 Type II, ISO 27001, and GDPR compliance audits. Perform vendor risk assessments and liaise with external auditing partners.",
                    Location = "Gurugram, India",
                    JobType = "Full Time",
                    Salary = "₹18,00,000 - ₹28,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.tcs@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                },

                // ==========================================
                // 7. MOBILE APPLICATION DEVELOPMENT (43 - 46)
                // ==========================================
                new Job
                {
                    Title = "Senior iOS Developer (Swift & SwiftUI)",
                    Description = "Craft fluid, responsive iOS apps using Swift 5.10+, SwiftUI, Combine, and MVVM-C architecture. Ensure high frame-rate performance, offline caching, and App Store guidelines compliance.",
                    Location = "Remote / Bangalore",
                    JobType = "Remote",
                    Salary = "₹24,00,000 - ₹38,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.swiggy@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Lead Android Engineer (Kotlin & Jetpack Compose)",
                    Description = "Lead Android development across high-volume delivery apps. Master Kotlin Coroutines, Flow, Hilt dependency injection, Room DB, and clean architecture.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹28,00,000 - ₹44,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.flipkart@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Job
                {
                    Title = "Senior Flutter Mobile Architect",
                    Description = "Deliver unified cross-platform mobile apps for iOS and Android using Flutter 3.22+, Dart, Bloc/Riverpod state management, and custom native platform channels.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$100,000 - $135,000 / yr",
                    RecruiterId = GetRecruiter("jobs.atlassian@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "React Native Developer",
                    Description = "Build cross-platform mobile applications with React Native, Expo, TypeScript, Redux Toolkit, and native module bridges. Optimize startup time and Hermes memory footprint.",
                    Location = "Pune / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹16,00,000 - ₹25,00,000 / yr",
                    RecruiterId = GetRecruiter("tech.razorpay@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },

                // ==========================================
                // 8. QA, SDET & AUTOMATION (47 - 50)
                // ==========================================
                new Job
                {
                    Title = "Lead QA Automation Engineer (Playwright & TypeScript)",
                    Description = "Design comprehensive end-to-end automation frameworks using Playwright, TypeScript, and Docker. Integrate tests into GitHub Actions pipelines for automated PR validation.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "₹20,00,000 - ₹32,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.atlassian@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Senior SDET (Software Development Engineer in Test - Java/Selenium)",
                    Description = "Build robust test automation suites for web and backend APIs using Java, Selenium WebDriver, TestNG, RestAssured, and Jenkins CI/CD.",
                    Location = "Hyderabad, India",
                    JobType = "Full Time",
                    Salary = "₹18,00,000 - ₹28,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.microsoft@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Performance & Load Test Engineer (k6, JMeter, Gatling)",
                    Description = "Benchmark distributed cloud services under peak traffic simulations. Identify database bottlenecks, memory leaks, and GC pauses with Grafana and APM tools.",
                    Location = "Noida / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹16,00,000 - ₹26,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.swiggy@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Job
                {
                    Title = "Mobile QA Automation Tester (Appium & BrowserStack)",
                    Description = "Automate regression testing on real devices across iOS and Android using Appium, Python/Java, and BrowserStack cloud execution grids.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹14,00,000 - ₹22,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.flipkart@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },

                // ==========================================
                // 9. DATABASE & DATA ADMINISTRATION (51 - 53)
                // ==========================================
                new Job
                {
                    Title = "Principal PostgreSQL Database Administrator (DBA)",
                    Description = "Oversee critical PostgreSQL database clusters. Manage replication, connection pooling (PgBouncer), query optimization, EXPLAIN ANALYZE tuning, and zero-downtime upgrades.",
                    Location = "Remote / Bangalore",
                    JobType = "Remote",
                    Salary = "₹28,00,000 - ₹45,00,000 / yr",
                    RecruiterId = GetRecruiter("jobs.stripe@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Job
                {
                    Title = "MongoDB & NoSQL Distributed Database Engineer",
                    Description = "Design sharded MongoDB replica sets and Redis clusters for high-velocity session management and real-time document stores across global regions.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹22,00,000 - ₹35,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.swiggy@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Job
                {
                    Title = "Microsoft SQL Server Database Specialist",
                    Description = "Maintain enterprise SQL Server instances with AlwaysOn Availability Groups, T-SQL stored procedures optimization, index maintenance, and disaster recovery.",
                    Location = "Pune / Hybrid",
                    JobType = "Full Time",
                    Salary = "₹18,00,000 - ₹28,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.microsoft@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                },

                // ==========================================
                // 10. PRODUCT, SCRUM & TECH MANAGEMENT (54 - 56)
                // ==========================================
                new Job
                {
                    Title = "Technical Product Manager (Developer Platform & APIs)",
                    Description = "Own the product roadmap for developer-facing APIs, SDKs, and developer documentation. Partner with backend architects and enterprise customers to drive developer adoption.",
                    Location = "Remote",
                    JobType = "Remote",
                    Salary = "$140,000 - $180,000 / yr",
                    RecruiterId = GetRecruiter("jobs.stripe@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Job
                {
                    Title = "Agile Scrum Master (Certified CSM / SAFe)",
                    Description = "Facilitate sprint planning, retrospectives, and standups for 3 cross-functional cloud engineering pods. Remove impediments and champion agile engineering excellence.",
                    Location = "Gurugram, India",
                    JobType = "Full Time",
                    Salary = "₹18,00,000 - ₹28,00,000 / yr",
                    RecruiterId = GetRecruiter("careers.tcs@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Engineering Manager (Core Infrastructure & SRE)",
                    Description = "Lead a team of 10+ cloud infrastructure and platform engineers. Guide technical direction, conduct 1-on-1s, drive hiring, and oversee SLA delivery for customer-facing services.",
                    Location = "Bangalore, India",
                    JobType = "Full Time",
                    Salary = "₹45,00,000 - ₹65,00,000 / yr",
                    RecruiterId = GetRecruiter("talent.google@hireflow.io"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            // Add all IT jobs to database
            context.Jobs.AddRange(itJobs);
            await context.SaveChangesAsync();
        }
    }
}
