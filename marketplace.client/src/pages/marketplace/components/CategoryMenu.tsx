
import { useState } from 'react';

interface CategoryMenuProps {
    onClose: () => void;
}

export default function CategoryMenu({ onClose }: CategoryMenuProps) {
    const [hoveredCategory, setHoveredCategory] = useState<string | null>(null);

    const categories = [
        {
            name: 'Electronics',
            icon: 'ri-smartphone-line',
            subcategories: [
                { name: 'Smartphones', items: ['iPhone', 'Samsung', 'Google Pixel', 'OnePlus'] },
                { name: 'Laptops', items: ['MacBook', 'Dell', 'HP', 'Lenovo'] },
                { name: 'Audio', items: ['Headphones', 'Speakers', 'Earbuds', 'Microphones'] },
                { name: 'Cameras', items: ['DSLR', 'Mirrorless', 'Action Cameras', 'Drones'] },
            ]
        },
        {
            name: 'Fashion',
            icon: 'ri-shirt-line',
            subcategories: [
                { name: 'Men\'s Clothing', items: ['Shirts', 'Pants', 'Jackets', 'Shoes'] },
                { name: 'Women\'s Clothing', items: ['Dresses', 'Tops', 'Skirts', 'Heels'] },
                { name: 'Accessories', items: ['Bags', 'Watches', 'Jewelry', 'Sunglasses'] },
                { name: 'Kids', items: ['Boys', 'Girls', 'Babies', 'Toys'] },
            ]
        },
        {
            name: 'Home & Garden',
            icon: 'ri-home-4-line',
            subcategories: [
                { name: 'Furniture', items: ['Sofas', 'Beds', 'Tables', 'Chairs'] },
                { name: 'Kitchen', items: ['Appliances', 'Cookware', 'Utensils', 'Storage'] },
                { name: 'Decor', items: ['Lighting', 'Rugs', 'Curtains', 'Wall Art'] },
                { name: 'Garden', items: ['Tools', 'Plants', 'Outdoor Furniture', 'BBQ'] },
            ]
        },
        {
            name: 'Sports & Outdoors',
            icon: 'ri-football-line',
            subcategories: [
                { name: 'Fitness', items: ['Gym Equipment', 'Yoga', 'Running', 'Cycling'] },
                { name: 'Outdoor', items: ['Camping', 'Hiking', 'Fishing', 'Climbing'] },
                { name: 'Team Sports', items: ['Football', 'Basketball', 'Baseball', 'Soccer'] },
                { name: 'Water Sports', items: ['Swimming', 'Surfing', 'Diving', 'Kayaking'] },
            ]
        },
        {
            name: 'Books & Media',
            icon: 'ri-book-open-line',
            subcategories: [
                { name: 'Books', items: ['Fiction', 'Non-Fiction', 'Educational', 'Comics'] },
                { name: 'Movies', items: ['Blu-ray', 'DVD', 'Digital', 'Collections'] },
                { name: 'Music', items: ['Vinyl', 'CDs', 'Digital', 'Instruments'] },
                { name: 'Games', items: ['Video Games', 'Board Games', 'Puzzles', 'Cards'] },
            ]
        },
        {
            name: 'Automotive',
            icon: 'ri-car-line',
            subcategories: [
                { name: 'Parts', items: ['Engine', 'Brakes', 'Suspension', 'Electrical'] },
                { name: 'Accessories', items: ['Interior', 'Exterior', 'Electronics', 'Tools'] },
                { name: 'Tires & Wheels', items: ['All Season', 'Winter', 'Summer', 'Rims'] },
                { name: 'Care', items: ['Cleaning', 'Maintenance', 'Detailing', 'Protection'] },
            ]
        },
    ];

    return (
        <div className="fixed inset-0 bg-black/50 z-50" onClick={onClose}>
            <div className="bg-white shadow-xl max-w-[1600px] mx-auto mt-2" onClick={(e) => e.stopPropagation()}>
                <div className="flex">
                    <div className="w-64 bg-gray-50 border-r border-gray-200">
                        {categories.map((category) => (
                            <div
                                key={category.name}
                                className="px-6 py-4 flex items-center gap-3 hover:bg-teal-50 cursor-pointer transition-colors border-b border-gray-200"
                                onMouseEnter={() => setHoveredCategory(category.name)}
                            >
                                <i className={`${category.icon} text-xl text-teal-500 w-6 h-6 flex items-center justify-center`}></i>
                                <span className="font-medium text-gray-900">{category.name}</span>
                                <i className="ri-arrow-right-s-line ml-auto text-gray-400"></i>
                            </div>
                        ))}
                    </div>

                    <div className="flex-1 p-8">
                        {hoveredCategory && (
                            <div>
                                <h3 className="text-2xl font-bold text-gray-900 mb-6">{hoveredCategory}</h3>
                                <div className="grid grid-cols-4 gap-8">
                                    {categories
                                        .find((c) => c.name === hoveredCategory)
                                        ?.subcategories.map((sub) => (
                                            <div key={sub.name}>
                                                <h4 className="font-semibold text-gray-900 mb-3">{sub.name}</h4>
                                                <ul className="space-y-2">
                                                    {sub.items.map((item) => (
                                                        <li key={item}>
                                                            <a href="#" className="text-sm text-gray-600 hover:text-teal-500 transition-colors cursor-pointer">
                                                                {item}
                                                            </a>
                                                        </li>
                                                    ))}
                                                </ul>
                                            </div>
                                        ))}
                                </div>
                            </div>
                        )}
                        {!hoveredCategory && (
                            <div className="flex items-center justify-center h-full text-gray-400">
                                <p>Hover over a category to see subcategories</p>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
