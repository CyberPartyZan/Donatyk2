export const orders = [
    {
        id: '1',
        orderNumber: '1001',
        date: '2025-01-20',
        status: 'Delivered',
        seller: {
            name: 'TechStore Pro',
            avatar: 'https://readdy.ai/api/search-image?query=professional%20tech%20store%20logo%20icon%20simple%20modern%20design&width=100&height=100&seq=seller1&orientation=squarish',
            rating: 4.8,
            reviews: 1250
        },
        delivery: {
            address: '123 Main Street, Apt 4B, New York, NY 10001',
            recipient: 'John Anderson'
        },
        items: [
            {
                id: '1',
                name: 'Premium Wireless Headphones with Active Noise Cancellation',
                price: 254.99,
                quantity: 1,
                image: 'https://readdy.ai/api/search-image?query=premium%20wireless%20headphones%20with%20noise%20cancellation%20product%20photo%20on%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=prod1&orientation=squarish'
            },
            {
                id: '5',
                name: 'Mechanical Gaming Keyboard RGB Backlit',
                price: 134.99,
                quantity: 1,
                image: 'https://readdy.ai/api/search-image?query=mechanical%20gaming%20keyboard%20with%20rgb%20lights%20product%20photo%20white%20background&width=400&height=400&seq=prod5&orientation=squarish'
            }
        ],
        payment: {
            method: 'Credit Card',
            status: 'Paid'
        },
        subtotal: 389.98,
        deliveryFee: 15.00,
        total: 404.98
    },
    {
        id: '2',
        orderNumber: '1002',
        date: '2025-01-18',
        status: 'In Transit',
        seller: {
            name: 'Camera World',
            avatar: 'https://readdy.ai/api/search-image?query=camera%20store%20logo%20icon%20photography%20theme&width=100&height=100&seq=seller3&orientation=squarish',
            rating: 4.9,
            reviews: 890
        },
        delivery: {
            address: '456 Oak Avenue, Suite 200, Los Angeles, CA 90012',
            recipient: 'Sarah Mitchell'
        },
        items: [
            {
                id: '3',
                name: 'Professional DSLR Camera with 24MP Sensor',
                price: 1039.20,
                quantity: 1,
                image: 'https://readdy.ai/api/search-image?query=professional%20dslr%20camera%20product%20shot%20on%20white%20background%20high%20quality%20photography%20equipment&width=400&height=400&seq=prod3&orientation=squarish'
            }
        ],
        payment: {
            method: 'PayPal',
            status: 'Paid'
        },
        subtotal: 1039.20,
        deliveryFee: 20.00,
        total: 1059.20
    },
    {
        id: '3',
        orderNumber: '1003',
        date: '2025-01-15',
        status: 'Processing',
        seller: {
            name: 'Electronics Depot',
            avatar: 'https://readdy.ai/api/search-image?query=electronics%20store%20logo%20icon%20tech%20retail&width=100&height=100&seq=seller7&orientation=squarish',
            rating: 4.7,
            reviews: 2100
        },
        delivery: {
            address: '789 Pine Road, Building C, Chicago, IL 60601',
            recipient: 'Michael Chen'
        },
        items: [
            {
                id: '7',
                name: '4K Ultra HD Smart TV 55 Inch',
                price: 559.30,
                quantity: 1,
                image: 'https://readdy.ai/api/search-image?query=4k%20ultra%20hd%20smart%20tv%20product%20shot%20clean%20white%20background%20modern%20television&width=400&height=400&seq=prod7&orientation=squarish'
            },
            {
                id: '10',
                name: 'Portable Bluetooth Speaker Waterproof',
                price: 71.99,
                quantity: 2,
                image: 'https://readdy.ai/api/search-image?query=portable%20bluetooth%20speaker%20waterproof%20product%20shot%20white%20background&width=400&height=400&seq=prod10&orientation=squarish'
            }
        ],
        payment: {
            method: 'Credit Card',
            status: 'Pending'
        },
        subtotal: 703.28,
        deliveryFee: 25.00,
        total: 728.28
    }
];
