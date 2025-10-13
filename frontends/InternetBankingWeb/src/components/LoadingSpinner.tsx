'use client';

import React from 'react';

interface LoadingSpinnerProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  color?: 'blue' | 'gray' | 'white';
  text?: string;
  fullScreen?: boolean;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'md',
  color = 'blue',
  text,
  fullScreen = false
}) => {
  const getSizeClasses = () => {
    switch (size) {
      case 'sm':
        return 'h-4 w-4';
      case 'md':
        return 'h-8 w-8';
      case 'lg':
        return 'h-12 w-12';
      case 'xl':
        return 'h-16 w-16';
      default:
        return 'h-8 w-8';
    }
  };

  const getColorClasses = () => {
    switch (color) {
      case 'blue':
        return 'border-blue-600';
      case 'gray':
        return 'border-gray-600';
      case 'white':
        return 'border-white';
      default:
        return 'border-blue-600';
    }
  };

  const spinner = (
    <div className="flex flex-col items-center justify-center">
      <div
        className={`animate-spin rounded-full border-b-2 ${getSizeClasses()} ${getColorClasses()}`}
      ></div>
      {text && (
        <p className={`mt-2 text-sm ${color === 'white' ? 'text-white' : 'text-gray-600'}`}>
          {text}
        </p>
      )}
    </div>
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 bg-white bg-opacity-75 flex items-center justify-center z-50">
        {spinner}
      </div>
    );
  }

  return spinner;
};

export default LoadingSpinner;
