export const mockOrganizations = [
    {
        id: 'org-1',
        name: 'Global Children\'s Education Fund',
        contactEmail: 'info@childreneducationfund.org',
        contactPhone: '+1 (555) 910-1112',
        description: 'Dedicated to providing quality education to underprivileged children across developing nations. We build schools, supply learning materials, and train local teachers to create sustainable educational infrastructure.',
        avatarImage: 'https://readdy.ai/api/search-image?query=charity%20organization%20logo%20for%20children%20education%20fund%20with%20colorful%20book%20and%20globe%20design%20minimalist%20modern%20nonprofit%20branding%20clean%20background&width=400&height=400&seq=org-avatar-001&orientation=squarish',
        createdAt: '2024-03-10T08:00:00',
        totalGoals: 3,
        completedGoals: 1
    },
    {
        id: 'org-2',
        name: 'Clean Water Initiative',
        contactEmail: 'hello@cleanwaterinit.org',
        contactPhone: '+1 (555) 814-1516',
        description: 'Focused on bringing clean, safe drinking water to communities in need. We install water purification systems, drill wells, and educate communities on water sanitation and hygiene practices.',
        avatarImage: 'https://readdy.ai/api/search-image?query=clean%20water%20charity%20organization%20logo%20with%20water%20drop%20and%20hands%20design%20minimalist%20eco%20nonprofit%20branding%20clean%20background&width=400&height=400&seq=org-avatar-002&orientation=squarish',
        createdAt: '2024-05-22T10:30:00',
        totalGoals: 2,
        completedGoals: 1
    },
    {
        id: 'org-3',
        name: 'Wildlife Conservation Alliance',
        contactEmail: 'protect@wildlifealliance.org',
        contactPhone: '+1 (555) 718-1920',
        description: 'Protecting endangered species and their habitats through conservation programs, anti-poaching patrols, and community engagement. We work across 15 countries to preserve biodiversity for future generations.',
        avatarImage: 'https://readdy.ai/api/search-image?query=wildlife%20conservation%20organization%20logo%20with%20animal%20silhouette%20and%20green%20earth%20design%20minimalist%20nature%20nonprofit%20branding%20clean%20background&width=400&height=400&seq=org-avatar-003&orientation=squarish',
        createdAt: '2024-06-18T14:00:00',
        totalGoals: 2,
        completedGoals: 0
    }
];

export const mockGoals = [
    {
        id: 'goal-1',
        organizationId: 'org-1',
        organizationName: 'Global Children\'s Education Fund',
        title: 'Build a School in Rural Kenya',
        explanation: 'Construct a fully-equipped primary school in the rural Makueni County of Kenya. The school will include 8 classrooms, a library, clean water facilities, and solar power. This project will provide quality education to over 300 children annually who currently walk over 5km to the nearest school.',
        approvementDocuments: [
            { name: 'Project Proposal - Kenya School Build 2025', url: '#', type: 'PDF', size: '2.4 MB' },
            { name: 'Land Ownership Certificate', url: '#', type: 'PDF', size: '1.1 MB' },
            { name: 'Construction Blueprint & Budget Breakdown', url: '#', type: 'PDF', size: '3.8 MB' }
        ],
        timeLimitStart: '2025-01-01T00:00:00',
        timeLimitEnd: '2025-12-31T23:59:59',
        moneyBudget: 85000,
        moneyRaised: 62300,
        status: 'Active',
        createdAt: '2024-12-01T09:00:00',
        linkedLots: ['1', '2'],
        charityDirection: 'Education, Science and Youth Development',
        parentGoalId: null
    },
    {
        id: 'goal-2',
        organizationId: 'org-1',
        organizationName: 'Global Children\'s Education Fund',
        title: 'Digital Literacy Program for Girls',
        explanation: 'Launch a digital literacy initiative providing 500 girls across 10 schools in rural India with computer access, coding workshops, and internet connectivity. Program aims to bridge the gender gap in STEM education and empower young women with future-ready skills.',
        approvementDocuments: [
            { name: 'Digital Literacy Program Proposal', url: '#', type: 'PDF', size: '1.8 MB' },
            { name: 'Partner School Agreements', url: '#', type: 'PDF', size: '2.2 MB' },
            { name: 'Curriculum & Workshop Schedule', url: '#', type: 'PDF', size: '1.5 MB' }
        ],
        timeLimitStart: '2025-03-01T00:00:00',
        timeLimitEnd: '2025-06-30T23:59:59',
        moneyBudget: 45000,
        moneyRaised: 17800,
        status: 'Active',
        createdAt: '2025-01-15T10:00:00',
        linkedLots: ['3'],
        charityDirection: 'Education, Science and Youth Development',
        parentGoalId: null
    },
    {
        id: 'goal-3',
        organizationId: 'org-1',
        organizationName: 'Global Children\'s Education Fund',
        title: 'Scholarship Fund for 50 Students',
        explanation: 'Provide full-year scholarships covering tuition, books, uniforms, and meals for 50 promising students from low-income families in the Philippines. The fund supports students from elementary through high school, removing financial barriers to education.',
        approvementDocuments: [
            { name: 'Scholarship Program Outline 2025', url: '#', type: 'PDF', size: '1.2 MB' },
            { name: 'Student Selection Criteria & Process', url: '#', type: 'PDF', size: '0.9 MB' }
        ],
        timeLimitStart: null,
        timeLimitEnd: null,
        moneyBudget: 25000,
        moneyRaised: 25000,
        status: 'Reached',
        createdAt: '2024-10-20T08:30:00',
        linkedLots: ['4'],
        charityDirection: 'Education, Science and Youth Development',
        parentGoalId: null
    },
    {
        id: 'goal-4',
        organizationId: 'org-2',
        organizationName: 'Clean Water Initiative',
        title: 'Water Well Project - Ethiopia',
        explanation: 'Drill and install 10 deep water wells across rural Ethiopian villages serving approximately 15,000 people. Each well includes a hand pump, concrete foundation, and community training on maintenance. Project reduces waterborne diseases and saves women and children hours of daily water collection.',
        approvementDocuments: [
            { name: 'Ethiopia Well Project Plan', url: '#', type: 'PDF', size: '2.6 MB' },
            { name: 'Geological Survey Reports', url: '#', type: 'PDF', size: '4.1 MB' },
            { name: 'Community Agreements & Permits', url: '#', type: 'PDF', size: '1.7 MB' }
        ],
        timeLimitStart: '2025-02-01T00:00:00',
        timeLimitEnd: '2025-08-31T23:59:59',
        moneyBudget: 42000,
        moneyRaised: 42000,
        status: 'Reached',
        createdAt: '2024-11-15T11:00:00',
        linkedLots: ['5'],
        charityDirection: 'Ecology and Community Reconstruction (Urban Studies)',
        parentGoalId: null
    },
    {
        id: 'goal-5',
        organizationId: 'org-2',
        organizationName: 'Clean Water Initiative',
        title: 'Rainwater Harvesting Systems - Guatemala',
        explanation: 'Install rainwater harvesting and filtration systems for 200 households in highland Guatemala communities. Each system captures and purifies rainwater for drinking, cooking, and hygiene, reducing dependency on contaminated surface water sources that cause frequent illness.',
        approvementDocuments: [
            { name: 'Guatemala Rainwater Project Proposal', url: '#', type: 'PDF', size: '1.9 MB' },
            { name: 'System Design Specifications', url: '#', type: 'PDF', size: '2.3 MB' }
        ],
        timeLimitStart: '2025-04-01T00:00:00',
        timeLimitEnd: '2025-10-31T23:59:59',
        moneyBudget: 35000,
        moneyRaised: 12100,
        status: 'Active',
        createdAt: '2025-02-10T14:00:00',
        linkedLots: [],
        charityDirection: 'Ecology and Community Reconstruction (Urban Studies)',
        parentGoalId: null
    },
    {
        id: 'goal-6',
        organizationId: 'org-3',
        organizationName: 'Wildlife Conservation Alliance',
        title: 'Anti-Poaching Patrol Equipment',
        explanation: 'Fund essential anti-poaching equipment for ranger teams protecting African elephants in Tanzania\'s Serengeti ecosystem. Equipment includes night-vision drones, GPS tracking collars, field medical kits, and vehicle maintenance for patrol jeeps.',
        approvementDocuments: [
            { name: 'Anti-Poaching Equipment Proposal', url: '#', type: 'PDF', size: '2.8 MB' },
            { name: 'Serengeti Patrol Zone Map', url: '#', type: 'PDF', size: '1.4 MB' },
            { name: 'Equipment Supplier Quotes', url: '#', type: 'PDF', size: '0.8 MB' }
        ],
        timeLimitStart: '2025-01-15T00:00:00',
        timeLimitEnd: '2025-07-15T23:59:59',
        moneyBudget: 55000,
        moneyRaised: 29300,
        status: 'Active',
        createdAt: '2024-12-20T09:00:00',
        linkedLots: ['6'],
        charityDirection: 'Ecology and Community Reconstruction (Urban Studies)',
        parentGoalId: null
    },
    {
        id: 'goal-7',
        organizationId: 'org-3',
        organizationName: 'Wildlife Conservation Alliance',
        title: 'Coral Reef Restoration - Great Barrier Reef',
        explanation: 'Support coral reef restoration efforts along 5 kilometers of the Great Barrier Reef. Project includes coral fragment nurseries, reef cleaning dives, water quality monitoring stations, and community education programs on reef preservation.',
        approvementDocuments: [
            { name: 'Reef Restoration Project Plan', url: '#', type: 'PDF', size: '3.2 MB' },
            { name: 'Marine Biology Impact Assessment', url: '#', type: 'PDF', size: '2.7 MB' },
            { name: 'Diving Operation Safety Protocol', url: '#', type: 'PDF', size: '1.1 MB' }
        ],
        timeLimitStart: '2025-05-01T00:00:00',
        timeLimitEnd: '2026-04-30T23:59:59',
        moneyBudget: 72000,
        moneyRaised: 8100,
        status: 'Active',
        createdAt: '2025-03-01T15:00:00',
        linkedLots: [],
        charityDirection: 'Ecology and Community Reconstruction (Urban Studies)',
        parentGoalId: null
    },
    {
        id: 'goal-8',
        organizationId: 'org-1',
        organizationName: 'Global Children\'s Education Fund',
        title: 'Solar Panel Installation for Kenya School',
        explanation: 'Install solar panels on the newly constructed school in Kenya to provide sustainable electricity for classrooms, computer labs, and lighting. This sub-project ensures the school can operate independently of the unreliable local power grid.',
        approvementDocuments: [
            { name: 'Solar Installation Quote', url: '#', type: 'PDF', size: '1.6 MB' }
        ],
        timeLimitStart: '2025-11-01T00:00:00',
        timeLimitEnd: '2026-01-31T23:59:59',
        moneyBudget: 15000,
        moneyRaised: 3200,
        status: 'Active',
        createdAt: '2025-10-01T12:00:00',
        linkedLots: [],
        charityDirection: 'Education, Science and Youth Development',
        parentGoalId: 'goal-1'
    }
];

export const defaultGoalId = 'goal-1';