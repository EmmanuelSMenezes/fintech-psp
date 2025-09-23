'use client';

import React from 'react';
import MaskedInput from './MaskedInput';

interface MaskedInputFieldProps {
  label: string;
  field: string;
  value: string;
  onChange: (field: string, value: string) => void;
  mask: 'document' | 'phone' | 'cep' | 'bankAccount' | 'bankAgency' | 'bankCode';
  required?: boolean;
  placeholder?: string;
}

const MaskedInputField: React.FC<MaskedInputFieldProps> = React.memo(({ 
  label, 
  field, 
  value,
  onChange,
  mask,
  required = false, 
  placeholder = ''
}) => {
  const handleChange = (maskedValue: string) => {
    onChange(field, maskedValue);
  };

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-2">
        {label} {required && <span className="text-red-500">*</span>}
      </label>
      <MaskedInput
        mask={mask}
        value={value}
        onChange={handleChange}
        required={required}
        placeholder={placeholder}
      />
    </div>
  );
});

MaskedInputField.displayName = 'MaskedInputField';

export default MaskedInputField;
