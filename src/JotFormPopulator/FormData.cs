static class FormData
{
    public static readonly string[] Names =
    [
        "Alice Hartman", "Ben Okafor", "Cara Delgado", "David Nguyen", "Elena Kowalski",
        "Felix Osei", "Grace Tanaka", "Hugo Ferreira", "Isla MacDonald", "James Okonkwo",
        "Kira Patel", "Luca Bianchi", "Maya Johansson", "Nate Rivera", "Olivia Muller",
        "Pablo Santos", "Quinn Larsen", "Rosa Yamamoto", "Sam Adeyemi", "Tara Lindqvist",
        "Aaron Petrov", "Beatriz Carvalho", "Chen Wei", "Diana Okoye", "Emeka Nwosu",
        "Fatima Al-Hassan", "Gabriel Moreau", "Hana Suzuki", "Ivan Novak", "Jade Kimura",
        "Kofi Mensah", "Layla Mansouri", "Marco Esposito", "Nadia Volkov", "Omar Khalil",
        "Priya Sharma", "Rafael Medina", "Sofia Andersen", "Tariq Hussain", "Uma Krishnan",
        "Victor Osei", "Wendy Nakamura", "Xander Dubois", "Yara Al-Rashid", "Zoe Papadopoulos",
        "Andre Dupont", "Blessing Eze", "Carlos Reyes", "Danya Petrov", "Esme Hartley",
        "Faisal Ibrahim", "Giulia Romano", "Hassan Diallo", "Ingrid Bergstrom", "Javier Ruiz",
        "Keiko Watanabe", "Leila Nazari", "Mohammed Saleh", "Nina Johansson", "Oluwaseun Adebayo",
        "Petra Kovac", "Rashid Okafor", "Suki Tanaka", "Tobias Fischer", "Uchenna Obi",
        "Valentina Cruz", "Wei Zhang", "Xiomara Flores", "Yusuf Abubakar", "Zara OBrien",
    ];

    public static readonly string[] EmailProviders =
    [
        "gmail.com", "hotmail.com", "yahoo.com", "outlook.com", "aol.com", "compuserve.com", "msn.com", "icloud.com",
    ];

    public static readonly string[] Ftse100Companies =
    [
        "3i", "Aberdeen Group", "Admiral Group", "Airtel Africa", "Alliance Witan",
        "Anglo American", "Antofagasta", "Associated British Foods", "AstraZeneca", "Auto Trader Group",
        "Aviva", "Babcock International", "BAE Systems", "Barclays", "Barratt Redrow",
        "Beazley", "BP", "British American Tobacco", "British Land", "BT Group",
        "Bunzl", "Burberry Group", "Centrica", "Coca-Cola Europacific Partners", "Coca-Cola HBC",
        "Compass Group", "Computacenter", "ConvaTec", "Croda International", "DCC",
        "Diageo", "Diploma", "Endeavour Mining", "Entain", "Experian",
        "F&C Investment Trust", "Fresnillo", "Games Workshop", "Glencore", "GSK",
        "Haleon", "Halma", "Hiscox", "Howdens Joinery", "HSBC",
        "ICG", "IG Group", "IHG Hotels & Resorts", "IMI", "Imperial Brands",
        "Informa", "International Airlines Group", "Intertek", "Investec", "JD Sports",
        "Kingfisher", "Land Securities", "Legal & General", "Lion Finance Group", "Lloyds Banking Group",
        "London Stock Exchange Group", "LondonMetric Property", "M&G", "Marks & Spencer", "Melrose Industries",
        "Metlen Energy & Metals", "National Grid", "NatWest Group", "Next", "Pearson",
        "Pershing Square Holdings", "Persimmon", "Polar Capital Technology Trust", "Prudential", "Reckitt",
        "RELX", "Rentokil Initial", "Rio Tinto", "Rolls-Royce Holdings", "Sage Group",
        "Sainsbury's", "Schroders", "Scottish Mortgage Investment Trust", "Segro", "Severn Trent",
        "Shell", "Smith & Nephew", "Smiths Group", "Spirax Group", "SSE",
        "Standard Chartered", "Standard Life", "St. James's Place", "Tesco", "Tritax Big Box REIT",
        "Unilever", "United Utilities", "Vodafone Group", "Weir Group", "Whitbread",
    ];

    public static readonly string[] GenderIdentities =
    [
        "Abinary", "Agender", "Agenderfluid", "Agenderflux", "Genderblank", "Genderfree", "Gendervoid",
        "Polyagender", "Ambigender", "Androgyne", "Androgynous", "Aporagender", "Autigender", "Autonomique",
        "Bakla", "Bigender", "Binary", "Bissu", "Butch", "Caelgender", "Calabai", "Calalai",
        "Cisgender", "Cis female", "Cis male", "Cis man", "Cis woman", "Colorgender", "Crystagender",
        "Demi-boy", "Demiflux", "Demigender", "Demi-girl", "Demi-guy", "Demi-man", "Demi-woman",
        "Dual gender", "Egogender", "Eunuch", "Faʻafafine", "Female", "Female to male", "Femme", "FTM",
        "Gender bender", "Gender diverse", "Gender gifted", "Genderfae", "Genderfaun", "Genderfluid",
        "Genderflux", "Genderfuck", "Genderless", "Gender nonconforming", "Genderpunk", "Genderqueer",
        "Gender questioning", "Gender variant", "Graygender", "Hijra", "Intergender", "Intersex",
        "Ipsogender", "Juxera", "Kathoey", "Kingender", "Leogender", "Lykh", "Māhū",
        "Male", "Male to female", "Man", "Man of trans experience", "Maverique", "Meta-gender",
        "Monogender", "MTF", "Multigender", "Muxe", "Neither", "Neurogender", "Neutrois",
        "Non-binary", "Non-binary man", "Non-binary transgender", "Non-binary woman", "Omnigender",
        "Other", "Outherine", "Pangender", "Person of transgendered experience", "Polygender",
        "Proxvir", "Queer", "Quoigender", "Sapphogender", "Sekhet", "Stargender", "Staticgender",
        "Third gender", "Trans", "Trans*", "Trans female", "Trans male", "Trans man", "Transnull",
        "Trans person", "Trans woman", "Transgender", "Transgender female", "Transgender male",
        "Transgender man", "Transgender person", "Transgender woman", "Transfeminine", "Transmasculine",
        "Transandrogynous", "Transsexual", "Transsexual female", "Transsexual male", "Transsexual man",
        "Transsexual person", "Transsexual woman", "Travesti", "Trigender", "Tumtum", "Two spirit",
        "Unigender", "Vakasalewalewa", "Waria", "Winkte", "Woman", "Woman of trans experience",
        "X-gender", "X-jendā", "Xenogenders",
    ];

    public static readonly string[] JobTitles =
    [
        "Dentist", "Registered Nurse", "Pharmacist", "Computer Systems Analyst", "Physician",
        "Database Administrator", "Software Developer", "Physical Therapist", "Web Developer", "Dental Hygienist",
        "Occupational Therapist", "Veterinarian", "Computer Programmer", "School Psychologist", "Physical Therapist Assistant",
        "Interpreter & Translator", "Mechanical Engineer", "Veterinary Technologist & Technician", "Epidemiologist", "IT Manager",
        "Market Research Analyst", "Diagnostic Medical Sonographer", "Computer Systems Administrator", "Respiratory Therapist", "Medical Secretary",
        "Civil Engineer", "Substance Abuse Counselor", "Speech-Language Pathologist", "Landscaper & Groundskeeper", "Radiologic Technologist",
        "Cost Estimator", "Financial Advisor", "Marriage & Family Therapist", "Medical Assistant", "Lawyer",
        "Accountant", "Compliance Officer", "High School Teacher", "Clinical Laboratory Technician", "Maintenance & Repair Worker",
        "Bookkeeping, Accounting & Audit Clerk", "Financial Manager", "Recreation & Fitness Worker", "Insurance Agent", "Elementary School Teacher",
        "Dental Assistant", "Management Analyst", "Home Health Aide", "Pharmacy Technician", "Construction Manager",
        "Public Relations Specialist", "Middle School Teacher", "Massage Therapist", "Paramedic", "Preschool Teacher",
        "Hairdresser", "Marketing Manager", "Patrol Officer", "School Counselor", "Executive Assistant",
        "Financial Analyst", "Personal Care Aide", "Clinical Social Worker", "Business Operations Manager", "Loan Officer",
        "Meeting, Convention & Event Planner", "Mental Health Counselor", "Nursing Aide", "Sales Representative", "Architect",
        "Sales Manager", "HR Specialist", "Plumber", "Real Estate Agent", "Glazier",
        "Art Director", "Customer Service Representative", "Logistician", "Auto Mechanic", "Bus Driver",
        "Restaurant Cook", "Child & Family Social Worker", "Administrative Assistant", "Receptionist", "Paralegal",
        "Cement Mason & Concrete Finisher", "Painter", "Sports Coach", "Teacher Assistant", "Brickmason & Blockmason",
        "Cashier", "Janitor", "Electrician", "Delivery Truck Driver", "Maid & Housekeeper",
        "Carpenter", "Security Guard", "Construction Worker", "Fabricator", "Telemarketer",
    ];

    public static readonly string[] Cities =
    [
        "New York, USA", "Los Angeles, USA", "Chicago, USA", "Houston, USA", "Phoenix, USA",
        "London, UK", "Birmingham, UK", "Manchester, UK", "Glasgow, UK", "Leeds, UK",
        "Toronto, Canada", "Vancouver, Canada", "Montreal, Canada", "Calgary, Canada",
        "Sydney, Australia", "Melbourne, Australia", "Brisbane, Australia", "Perth, Australia",
        "Lagos, Nigeria", "Abuja, Nigeria", "Accra, Ghana", "Nairobi, Kenya", "Johannesburg, South Africa",
        "Cape Town, South Africa", "Dar es Salaam, Tanzania", "Addis Ababa, Ethiopia",
        "Mumbai, India", "Delhi, India", "Bangalore, India", "Hyderabad, India", "Chennai, India",
        "Lahore, Pakistan", "Karachi, Pakistan", "Dhaka, Bangladesh", "Colombo, Sri Lanka",
        "Dubai, UAE", "Abu Dhabi, UAE", "Riyadh, Saudi Arabia", "Doha, Qatar", "Kuwait City, Kuwait",
        "Kuala Lumpur, Malaysia", "Singapore", "Manila, Philippines", "Jakarta, Indonesia",
        "Bangkok, Thailand", "Ho Chi Minh City, Vietnam", "Yangon, Myanmar",
        "Berlin, Germany", "Paris, France", "Madrid, Spain", "Rome, Italy", "Amsterdam, Netherlands",
        "Brussels, Belgium", "Warsaw, Poland", "Bucharest, Romania", "Istanbul, Turkey",
        "Cairo, Egypt", "Casablanca, Morocco", "Tunis, Tunisia", "Algiers, Algeria",
        "São Paulo, Brazil", "Rio de Janeiro, Brazil", "Buenos Aires, Argentina", "Lima, Peru",
        "Bogotá, Colombia", "Santiago, Chile", "Mexico City, Mexico",
        "Beijing, China", "Shanghai, China", "Guangzhou, China", "Shenzhen, China",
        "Tokyo, Japan", "Osaka, Japan", "Seoul, South Korea",
    ];

    public static readonly string[] Countries =
    [
        "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina", "Armenia",
        "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus",
        "Belgium", "Belize", "Benin", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil",
        "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia", "Cameroon", "Canada",
        "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros", "Congo", "Costa Rica",
        "Croatia", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic",
        "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini", "Ethiopia",
        "Fiji", "Finland", "France", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada",
        "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India",
        "Indonesia", "Iran", "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan",
        "Kenya", "Kiribati", "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia",
        "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar", "Malawi", "Malaysia", "Maldives",
        "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova",
        "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal",
        "Netherlands", "New Zealand", "Nicaragua", "Niger", "Nigeria", "North Korea", "North Macedonia",
        "Norway", "Oman", "Pakistan", "Palau", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru",
        "Philippines", "Poland", "Portugal", "Qatar", "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis",
        "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe",
        "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia",
        "Solomon Islands", "Somalia", "South Africa", "South Korea", "South Sudan", "Spain", "Sri Lanka",
        "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Taiwan", "Tajikistan", "Tanzania", "Thailand",
        "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Tuvalu",
        "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States", "Uruguay", "Uzbekistan",
        "Vanuatu", "Vatican City", "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe",
    ];
}
