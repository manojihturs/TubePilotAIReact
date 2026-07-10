import { useEffect, useState } from 'react';
import { BookOpen, Zap } from 'lucide-react';
import { promptCategoryService } from '../services/promptCategoryService';
import { promptService } from '../services/promptService';
import { promptVariableService } from '../services/promptVariableService';
import type { PromptCategory } from '../services/promptCategoryService';
import { Link } from 'react-router-dom';

export default function Dashboard() {
    const [categories, setCategories] = useState<PromptCategory[]>([]);
    const [promptsCount, setPromptsCount] = useState(0);
    const [variablesCount, setVariablesCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const [categoriesData, promptsData, variablesData] = await Promise.all([
                promptCategoryService.getAll(),
                promptService.getAll(),
                promptVariableService.getAll(),
            ]);
            setCategories(categoriesData);
            setPromptsCount(promptsData.length);
            setVariablesCount(variablesData.length);
            setError(null);
        } catch (err) {
            console.error('Failed to load dashboard data:', err);
            setError('Failed to load dashboard data');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="p-4 sm:p-6 lg:p-8">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="mb-12">
                    <h1 className="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white mb-2">
                        Dashboard
                    </h1>
                    <p className="text-gray-600 dark:text-gray-400">
                        Manage your prompts and content creation workflow
                    </p>
                </div>

                {/* Quick Stats - Real Data from API */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
                    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 border border-gray-200 dark:border-gray-700 hover:shadow-md transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 text-sm font-medium">Total Prompts</p>
                                <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2">
                                    {promptsCount}
                                </p>
                            </div>
                            <BookOpen className="text-blue-500" size={32} />
                        </div>
                    </div>

                    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 border border-gray-200 dark:border-gray-700 hover:shadow-md transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 text-sm font-medium">Categories</p>
                                <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2">
                                    {categories.length}
                                </p>
                            </div>
                            <BookOpen className="text-purple-500" size={32} />
                        </div>
                    </div>

                    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 border border-gray-200 dark:border-gray-700 hover:shadow-md transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 text-sm font-medium">Variables</p>
                                <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2">
                                    {variablesCount}
                                </p>
                            </div>
                            <Zap className="text-yellow-500" size={32} />
                        </div>
                    </div>
                </div>

                {/* Prompt Categories Section */}
                <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
                    <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                        <div className="flex items-center justify-between">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Prompt Categories</h2>
                            <Link
                                to="/prompt-categories"
                                className="text-sm text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                            >
                                View All
                            </Link>
                        </div>
                    </div>

                    <div className="p-6">
                        {loading && (
                            <div className="text-center py-8">
                                <p className="text-gray-500 dark:text-gray-400">Loading data...</p>
                            </div>
                        )}

                        {error && (
                            <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
                                <p className="text-red-700 dark:text-red-400">{error}</p>
                                <button 
                                    onClick={loadData} 
                                    className="mt-2 text-sm text-red-600 dark:text-red-400 hover:underline font-medium"
                                >
                                    Try Again
                                </button>
                            </div>
                        )}

                        {!loading && !error && categories.length === 0 && (
                            <div className="text-center py-8">
                                <p className="text-gray-500 dark:text-gray-400 mb-4">No categories yet</p>
                                <Link
                                    to="/prompt-categories"
                                    className="inline-block px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
                                >
                                    Create First Category
                                </Link>
                            </div>
                        )}

                        {!loading && !error && categories.length > 0 && (
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                {categories.slice(0, 6).map((category) => (
                                    <div
                                        key={category.id}
                                        className="p-4 rounded-lg border border-gray-200 dark:border-gray-700 hover:shadow-md hover:border-gray-300 dark:hover:border-gray-600 transition-all cursor-pointer"
                                    >
                                        <h3 className="font-semibold text-gray-900 dark:text-white mb-2">
                                            {category.name}
                                        </h3>
                                        {category.description && (
                                            <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-2">
                                                {category.description}
                                            </p>
                                        )}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                {/* Quick Actions */}
                <div className="mt-8 flex flex-col sm:flex-row gap-4">
                    <Link
                        to="/prompts"
                        className="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors text-center"
                    >
                        Create New Prompt
                    </Link>
                    <Link
                        to="/prompt-categories"
                        className="px-6 py-3 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors text-center"
                    >
                        Manage Categories
                    </Link>
                </div>
            </div>
        </div>
    );
}
