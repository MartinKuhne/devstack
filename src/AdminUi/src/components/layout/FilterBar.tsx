import { Input } from '@/components/ui/input';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Search, X } from 'lucide-react';
import type { ReactNode } from 'react';

export type FilterBarSelect = {
    label?: string;
    value: string;
    options: { value: string; label: string }[];
    onChange: (value: string) => void;
    placeholder?: string;
};

export type FilterBarProps = {
    searchValue?: string;
    onSearchChange?: (value: string) => void;
    onSearchSubmit?: () => void;
    onSearchClear?: () => void;
    selects?: FilterBarSelect[];
    filterSlot?: ReactNode;
    className?: string;
    searchPlaceholder?: string;
};

export function FilterBar({
    searchValue,
    onSearchChange,
    onSearchSubmit,
    onSearchClear,
    selects,
    filterSlot,
    className,
    searchPlaceholder,
}: FilterBarProps) {
    const hasSearch = searchValue !== undefined && onSearchChange !== undefined;

    return (
        <div className={className}>
            <div className="flex flex-wrap gap-4 items-end">
                {hasSearch && (
                    <form onSubmit={(e) => { e.preventDefault(); onSearchSubmit?.(); }} className="flex-1 min-w-[200px]">
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                            <Input
                                placeholder={searchPlaceholder ?? 'Search...'}
                                value={searchValue}
                                onChange={(e) => onSearchChange(e.target.value)}
                                className="pl-9 pr-9"
                            />
                            {searchValue && (
                                <button
                                    type="button"
                                    onClick={onSearchClear}
                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                                >
                                    <X className="h-4 w-4" />
                                </button>
                            )}
                        </div>
                    </form>
                )}

                {selects?.map((select, index) => (
                    <div key={index} className="w-48">
                        <Select
                            value={select.value}
                            onValueChange={select.onChange}
                        >
                            <SelectTrigger>
                                <SelectValue placeholder={select.placeholder ?? select.label} />
                            </SelectTrigger>
                            <SelectContent>
                                {select.options.map((option) => (
                                    <SelectItem key={option.value} value={option.value}>
                                        {option.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                ))}

                {filterSlot}
            </div>
        </div>
    );
}
